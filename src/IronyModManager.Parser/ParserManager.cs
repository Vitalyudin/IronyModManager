﻿// ***********************************************************************
// Assembly         : IronyModManager.Parser
// Author           : Mario
// Created          : 02-19-2020
//
// Last Modified By : Mario
// Last Modified On : 10-18-2024
// ***********************************************************************
// <copyright file="ParserManager.cs" company="Mario">
//     Mario
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IronyModManager.DI;
using IronyModManager.Parser.Common;
using IronyModManager.Parser.Common.Args;
using IronyModManager.Parser.Common.Parsers;
using IronyModManager.Shared.Models;
using ValueType = IronyModManager.Shared.Models.ValueType;

namespace IronyModManager.Parser
{
    /// <summary>
    /// Class ParserManager.
    /// Implements the <see cref="IronyModManager.Parser.Common.IParserManager" />
    /// </summary>
    /// <seealso cref="IronyModManager.Parser.Common.IParserManager" />
    public class ParserManager : IParserManager
    {
        #region Fields

        /// <summary>
        /// The default parsers
        /// </summary>
        private readonly IEnumerable<IDefaultParser> defaultParsers;

        /// <summary>
        /// The game parsers
        /// </summary>
        private readonly IEnumerable<IGameParser> gameParsers;

        /// <summary>
        /// The generic parsers
        /// </summary>
        private readonly IEnumerable<IGenericParser> genericParsers;

        /// <summary>
        /// The parser maps
        /// </summary>
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, List<IParserMap>>> parserMaps;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ParserManager" /> class.
        /// </summary>
        /// <param name="gameParsers">The game parsers.</param>
        /// <param name="genericParsers">The generic parsers.</param>
        /// <param name="defaultParsers">The default parsers.</param>
        public ParserManager(IEnumerable<IGameParser> gameParsers, IEnumerable<IGenericParser> genericParsers, IEnumerable<IDefaultParser> defaultParsers)
        {
            parserMaps = new ConcurrentDictionary<string, ConcurrentDictionary<string, List<IParserMap>>>();
            this.gameParsers = gameParsers.OrderBy(p => p.Priority);
            this.genericParsers = genericParsers.OrderBy(p => p.Priority);
            this.defaultParsers = defaultParsers;
            ValidateParserNames(gameParsers);
            ValidateParserNames(genericParsers);
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Parses the specified stream.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>IIndexedDefinitions.</returns>
        public IEnumerable<IDefinition> Parse(ParserManagerArgs args)
        {
            static bool isValidLine(string line, string commentId)
            {
                var text = line ?? string.Empty;
                return !string.IsNullOrWhiteSpace(text) && !text.Trim().StartsWith(commentId);
            }

            // Check if empty text file
            var commentId = args.File.EndsWith(Constants.LuaExtension, StringComparison.OrdinalIgnoreCase) ? Constants.Scripts.LuaScriptCommentId : Constants.Scripts.ScriptCommentId.ToString();
            if (!args.IsBinary && (args.Lines == null || !args.Lines.Any() || !args.Lines.Any(p => isValidLine(p, commentId))))
            {
                var definition = DIResolver.Get<IDefinition>();
                definition.OriginalCode = definition.Code = Comments.GetEmptyCommentType(args.File);
                definition.CodeSeparator = definition.CodeTag = string.Empty;
                definition.ContentSHA = args.ContentSHA;
                definition.Dependencies = args.ModDependencies;
                definition.File = args.File;
                definition.Id = Path.GetFileName(args.File).ToLowerInvariant();
                definition.Type = args.File.FormatDefinitionType();
                definition.ModName = args.ModName;
                definition.OriginalModName = args.ModName;
                definition.OriginalFileName = args.File;
                definition.UsedParser = string.Empty;
                definition.ValueType = ValueType.EmptyFile;
                definition.LastModified = args.FileLastModified;
                return new List<IDefinition> { definition };
            }

            return InvokeParsers(args);
        }

        /// <summary>
        /// Sets the parser.
        /// </summary>
        /// <param name="definitions">The definitions.</param>
        /// <param name="parserName">Name of the parser.</param>
        /// <param name="lastModified">The last modified.</param>
        private static void SetAdditionalData(IEnumerable<IDefinition> definitions, string parserName, DateTime? lastModified)
        {
            if (definitions?.Count() > 0)
            {
                var order = 0;
                foreach (var item in definitions)
                {
                    if (item.ValueType != ValueType.Variable && item.ValueType != ValueType.Namespace)
                    {
                        order++;
                        item.Order = order;
                    }

                    item.UsedParser = parserName;
                    item.LastModified = lastModified;
                }
            }
        }

        /// <summary>
        /// Validates the parser names.
        /// </summary>
        /// <param name="parsers">The parsers.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Duplicate parsers detected: {message}</exception>
        private static void ValidateParserNames(IEnumerable<IDefaultParser> parsers)
        {
            var invalid = parsers.GroupBy(p => p.ParserName).Where(s => s.Count() > 1);
            if (invalid.Any())
            {
                var message = string.Join(',', invalid.SelectMany(s => s).Select(s => s.ParserName));
                throw new ArgumentOutOfRangeException($"Duplicate parsers detected: {message}");
            }
        }

        /// <summary>
        /// Evaluates for placeholders.
        /// </summary>
        /// <param name="definitions">The definitions.</param>
        /// <param name="lines">The lines.</param>
        private void EvaluateForPlaceholders(IEnumerable<IDefinition> definitions, IEnumerable<string> lines)
        {
            if (lines != null)
            {
                if (lines.Any(p => !string.IsNullOrEmpty(p) && p.Contains(Constants.Scripts.PlaceholderFileComment, StringComparison.OrdinalIgnoreCase)))
                {
                    foreach (var item in definitions)
                    {
                        item.IsPlaceholder = true;
                    }
                }
                else if (lines.Any(p => !string.IsNullOrEmpty(p) && p.Contains(Constants.Scripts.PlaceholderObjectsComment, StringComparison.OrdinalIgnoreCase)))
                {
                    var placeholderLine = lines.FirstOrDefault(p => p.Contains(Constants.Scripts.PlaceholderObjectsComment, StringComparison.OrdinalIgnoreCase));
                    var values = placeholderLine?.Split(':');
                    if (values is { Length: 2 })
                    {
                        var ids = values[1].Split(',');
                        var cleanedIds = new List<string>();
                        ids.ToList().ForEach(p => cleanedIds.Add(p.Trim()));
                        foreach (var item in definitions)
                        {
                            if (cleanedIds.Any(i => i.Equals(item.Id, StringComparison.OrdinalIgnoreCase)))
                            {
                                item.IsPlaceholder = true;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the preferred parser.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="location">The location.</param>
        /// <returns>System.String.</returns>
        private IEnumerable<string> GetPreferredParsers(string game, string location)
        {
            location = location.ToLowerInvariant();
            IEnumerable<string> parser = null;
            if (parserMaps.TryGetValue(game, out var maps))
            {
                if (maps.TryGetValue(location, out var value))
                {
                    parser = value.Select(p => p.PreferredParser);
                }
            }
            else
            {
                var path = string.Format(Constants.ParserMapPath, game);
                if (File.Exists(path))
                {
                    var content = File.ReadAllText(path);
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        var cachedMap = JsonDISerializer.Deserialize<List<IParserMap>>(content);
                        var grouped = cachedMap.GroupBy(p => p.DirectoryPath);
                        var newMaps = new ConcurrentDictionary<string, List<IParserMap>>();
                        foreach (var item in grouped)
                        {
                            var id = item.First().DirectoryPath.ToLowerInvariant();
                            newMaps.TryAdd(id, item.Select(p => p).ToList());
                            if (id.Equals(location))
                            {
                                parser = item.Select(p => p.PreferredParser);
                            }
                        }

                        parserMaps.TryAdd(game, newMaps);
                    }
                }
            }

            return parser;
        }

        /// <summary>
        /// Invokes the parsers.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>IEnumerable&lt;IDefinition&gt;.</returns>
        private IEnumerable<IDefinition> InvokeParsers(ParserManagerArgs args)
        {
            var canParseArgs = new CanParseArgs { File = args.File, GameType = args.GameType, Lines = args.Lines ?? new List<string>(), IsBinary = args.IsBinary };
            var parseArgs = new ParserArgs
            {
                ContentSHA = args.ContentSHA,
                ModDependencies = args.ModDependencies,
                File = args.File,
                Lines = args.Lines ?? new List<string>(),
                ModName = args.ModName,
                ValidationType = args.ValidationType,
                IsBinary = args.IsBinary,
                GameType = args.GameType
            };
            var preferredParserNames = GetPreferredParsers(args.GameType, Path.GetDirectoryName(args.File));
            IDefaultParser preferredParser = null;
            if (preferredParserNames?.Count() > 0)
            {
                var gameParser = gameParsers.Where(p => preferredParserNames.Any(s => s.Equals(p.ParserName)));
                if (gameParser.Any())
                {
                    preferredParser = gameParser.FirstOrDefault(p => p.CanParse(canParseArgs));
                }

                var genericParser = genericParsers.Where(p => preferredParserNames.Any(s => s.Equals(p.ParserName)));
                if (preferredParser == null && genericParser.Any())
                {
                    preferredParser = genericParser.FirstOrDefault(p => p.CanParse(canParseArgs));
                }

                var defaultParser = defaultParsers.Where(p => preferredParserNames.Any(s => s.Equals(p.ParserName)));
                if (preferredParser == null && defaultParsers.Any())
                {
                    preferredParser = defaultParser.FirstOrDefault(p => p.CanParse(canParseArgs));
                }
            }

            IEnumerable<IDefinition> result = null;

            // This will be auto generated when a game is scanned for the first time. It was rushed and is now generated via unit test and is no where near as completed.
            if (preferredParser != null)
            {
                result = preferredParser.Parse(parseArgs);
                SetAdditionalData(result, preferredParser.ParserName, args.FileLastModified);
            }
            else
            {
                var gameParser = gameParsers.FirstOrDefault(p => p.CanParse(canParseArgs));
                if (gameParser != null)
                {
                    result = gameParser.Parse(parseArgs);
                    SetAdditionalData(result, gameParser.ParserName, args.FileLastModified);
                }
                else
                {
                    var genericParser = genericParsers.FirstOrDefault(p => p.CanParse(canParseArgs));
                    if (genericParser != null)
                    {
                        result = genericParser.Parse(parseArgs);
                        SetAdditionalData(result, genericParser.ParserName, args.FileLastModified);
                    }
                    else
                    {
                        var parser = defaultParsers.FirstOrDefault(p => p.CanParse(canParseArgs));
                        result = parser?.Parse(parseArgs);
                        SetAdditionalData(result, parser?.ParserName, args.FileLastModified);
                    }
                }
            }

            EvaluateForPlaceholders(result, args.Lines);
            return result;
        }

        #endregion Methods
    }
}
