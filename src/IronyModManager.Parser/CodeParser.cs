﻿// ***********************************************************************
// Assembly         : IronyModManager.Parser
// Author           : Mario
// Created          : 02-22-2020
//
// Last Modified By : Mario
// Last Modified On : 02-06-2025
// ***********************************************************************
// <copyright file="CodeParser.cs" company="Mario">
//     Mario
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CWTools.CSharp;
using IronyModManager.DI;
using IronyModManager.Parser.Common.Parsers;
using IronyModManager.Parser.Common.Parsers.Models;
using IronyModManager.Shared;

namespace IronyModManager.Parser
{
    /// <summary>
    /// Class TextParser.
    /// Implements the <see cref="IronyModManager.Parser.Common.Parsers.ICodeParser" />
    /// </summary>
    /// <seealso cref="IronyModManager.Parser.Common.Parsers.ICodeParser" />
    [ExcludeFromCoverage("Code parser is tested in parser implementations.")]
    public class CodeParser : ICodeParser
    {
        #region Fields

        /// <summary>
        /// The trace back tolerance
        /// </summary>
        protected const int TraceBackTolerance = 100;

        /// <summary>
        /// The cleaner conversion map
        /// </summary>
        protected static readonly Dictionary<string, string> CleanerConversionMap = new()
        {
            { $" {Common.Constants.Scripts.EqualsOperator}", Common.Constants.Scripts.EqualsOperator.ToString() },
            { $"{Common.Constants.Scripts.EqualsOperator} ", Common.Constants.Scripts.EqualsOperator.ToString() },
            { $" {Common.Constants.Scripts.OpenObject}", Common.Constants.Scripts.OpenObject.ToString() },
            { $"{Common.Constants.Scripts.OpenObject} ", Common.Constants.Scripts.OpenObject.ToString() },
            { $" {Common.Constants.Scripts.CloseObject}", Common.Constants.Scripts.CloseObject.ToString() },
            { $"{Common.Constants.Scripts.CloseObject} ", Common.Constants.Scripts.CloseObject.ToString() }
        };

        /// <summary>
        /// The quotes regex
        /// </summary>
        // ReSharper disable once UseRawString
        protected static readonly Regex QuotesRegex = new(@"""(\\""|\\\\|[^""\\])*""", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger logger;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeParser" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public CodeParser(ILogger logger)
        {
            this.logger = logger;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Cleans the code.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="lines">The lines.</param>
        /// <returns>IEnumerable&lt;System.String&gt;.</returns>
        public virtual IEnumerable<string> CleanCode(string file, IEnumerable<string> lines)
        {
            var commentId = IsLua(file) ? Common.Constants.Scripts.LuaScriptCommentId : Common.Constants.Scripts.ScriptCommentId.ToString();
            var validCodeLines = lines.Where(p => !string.IsNullOrWhiteSpace(p) && !p.Trim().StartsWith(commentId)).Select(p => RemoveInlineComments(commentId, p));
            return FormatCurlyBraces(string.Join(Environment.NewLine, validCodeLines)).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Select(FormatCodeTerminators);
        }

        /// <summary>
        /// Cleans the whitespace.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <returns>System.String.</returns>
        public string CleanWhitespace(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                return string.Empty;
            }

            var cleaned = string.Join(' ', line.Trim().Replace("\t", " ").Split(' ', StringSplitOptions.RemoveEmptyEntries));
            foreach (var item in CleanerConversionMap)
            {
                cleaned = cleaned.Replace(item.Key, item.Value);
            }

            return cleaned;
        }

        /// <summary>
        /// Formats the code.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="indentLevel">The indent level.</param>
        /// <returns>System.String.</returns>
        public string FormatCode(IScriptElement element, int indentLevel = 0)
        {
            static string format(IScriptElement element, int indent, bool noLeadingSpace = false)
            {
                var sb = new StringBuilder();
                if (element.IsSimpleType)
                {
                    if (!string.IsNullOrWhiteSpace(element.Value))
                    {
                        if (!string.IsNullOrWhiteSpace(element.Operator))
                        {
                            sb.Append($"{new string(' ', indent * 4)}{element.Key} {element.Operator} {element.Value}");
                        }
                        else
                        {
                            sb.Append($"{new string(' ', indent * 4)}{element.Key} {element.Value}");
                        }
                    }
                    else
                    {
                        sb.Append($"{new string(' ', indent * 4)}{element.Key}");
                    }
                }
                else
                {
                    var inlineChildValues = element.Values?.Where(p => Common.Constants.Scripts.InlineOperators.Any(a => a.Equals(p.Key, StringComparison.OrdinalIgnoreCase)));
                    if (inlineChildValues?.Count() > 0 && inlineChildValues.Count() == element.Values?.Count())
                    {
                        if (!string.IsNullOrWhiteSpace(element.Operator))
                        {
                            sb.Append($"{new string(' ', indent * 4)}{element.Key} {element.Operator} ");
                        }
                        else
                        {
                            sb.Append($"{new string(' ', indent * 4)}{element.Key} ");
                        }

                        if (element.Values.Any())
                        {
                            foreach (var value in element.Values)
                            {
                                sb.Append(format(value, indent, true));
                            }
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(element.Operator))
                        {
                            if (noLeadingSpace)
                            {
                                sb.AppendLine($"{element.Key} {element.Operator} {Common.Constants.Scripts.OpenObject}");
                            }
                            else
                            {
                                sb.AppendLine($"{new string(' ', indent * 4)}{element.Key} {element.Operator} {Common.Constants.Scripts.OpenObject}");
                            }
                        }
                        else
                        {
                            if (noLeadingSpace)
                            {
                                sb.AppendLine($"{element.Key} {Common.Constants.Scripts.OpenObject}");
                            }
                            else
                            {
                                sb.AppendLine($"{new string(' ', indent * 4)}{element.Key} {Common.Constants.Scripts.OpenObject}");
                            }
                        }

                        if (element.Values?.Count() > 0)
                        {
                            foreach (var value in element.Values)
                            {
                                sb.AppendLine(format(value, indent + 1));
                            }
                        }

                        sb.Append($"{new string(' ', indent * 4)}{Common.Constants.Scripts.CloseObject}");
                    }
                }

                return sb.ToString();
            }

            if (element != null)
            {
                return format(element, indentLevel);
            }

            return string.Empty;
        }

        /// <summary>
        /// Determines whether the specified file is lua.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns><c>true</c> if the specified file is lua; otherwise, <c>false</c>.</returns>
        public bool IsLua(string file)
        {
            return file.EndsWith(Common.Constants.LuaExtension, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Parses the script.
        /// </summary>
        /// <param name="lines">The lines.</param>
        /// <param name="file">The file.</param>
        /// <param name="performSimpleCheck">if set to <c>true</c> [perform simple check].</param>
        /// <returns>IParseResponse.</returns>
        public IParseResponse ParseScript(IEnumerable<string> lines, string file, bool performSimpleCheck = false)
        {
            return ParseScriptData(lines, file, performSimpleCheck, false);
        }

        /// <summary>
        /// Parses the script without validation.
        /// </summary>
        /// <param name="lines">The lines.</param>
        /// <param name="file">The file.</param>
        /// <returns>IParseResponse.</returns>
        public IParseResponse ParseScriptWithoutValidation(IEnumerable<string> lines, string file)
        {
            return ParseScriptData(lines, file, skipValidityCheck: true);
        }

        /// <summary>
        /// Performs the validity check.
        /// </summary>
        /// <param name="lines">The lines.</param>
        /// <param name="file">The file.</param>
        /// <param name="performSimpleCheck">if set to <c>true</c> [perform simple check].</param>
        /// <returns>IScriptError.</returns>
        public IScriptError PerformValidityCheck(IEnumerable<string> lines, string file, bool performSimpleCheck = false)
        {
            if (performSimpleCheck)
            {
                var error = PerformBasicValidityCheck(file, lines);
                if (error != null)
                {
                    return error;
                }
            }
            else
            {
                var code = string.Join(Environment.NewLine, lines);
                var response = Parsers.ParseScriptFile(file, code);
                if (!response.IsSuccess)
                {
                    var errorResponse = response.GetError();
                    var error = DIResolver.Get<IScriptError>();
                    error.Column = errorResponse.Column;
                    error.Line = errorResponse.Line;
                    error.Message = errorResponse.ErrorMessage;
                    return error;
                }
            }

            return null;
        }

        /// <summary>
        /// Formats the code terminators.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <returns>System.String.</returns>
        protected string FormatCodeTerminators(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                return string.Empty;
            }

            var regexHits = QuotesRegex.Matches(line);
            var cleaned = string.Join(" ", line.Trim().Replace("\t", " ").Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
            if (regexHits.Count > 0)
            {
                var cleanedRegexHits = QuotesRegex.Matches(cleaned);
                for (var i = 0; i < regexHits.Count; i++)
                {
                    var insert = line.Substring(regexHits[i].Index, regexHits[i].Length);
                    cleaned = cleaned.Remove(cleanedRegexHits[i].Index, cleanedRegexHits[i].Length).Insert(cleanedRegexHits[i].Index, insert);
                }
            }

            return cleaned.Trim();
        }

        /// <summary>
        /// Formats the curly braces.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns>System.String.</returns>
        protected string FormatCurlyBraces(string code)
        {
            var sb = new StringBuilder();
            var quoteOpened = false;
            for (var i = 0; i < code.Length; i++)
            {
                var c = code[i];
                if (c == Common.Constants.Scripts.Quote)
                {
                    var canCloseQuote = true;
                    if (i - 1 > 0)
                    {
                        var prev = code[i - 1];
                        if (prev == Common.Constants.Scripts.EscapeCharacter)
                        {
                            canCloseQuote = false;
                        }
                    }

                    if (quoteOpened && canCloseQuote)
                    {
                        quoteOpened = false;
                    }
                    else
                    {
                        quoteOpened = true;
                    }
                }

                var addLine = true;
                if (c == Common.Constants.Scripts.OpenObject || c == Common.Constants.Scripts.CloseObject)
                {
                    if (!quoteOpened)
                    {
                        sb.Append($" {c} ");
                        addLine = false;
                    }
                }

                if (addLine)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets the element.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="index">The index.</param>
        /// <returns>IScriptElement.</returns>
        protected IScriptElement GetElement(List<char> code, ref int index)
        {
            IgnoreElementWhiteSpace(code, ref index);
            var elKey = GetElementValue(code, ref index, true);
            char? elOperator;
            if (!string.IsNullOrWhiteSpace(elKey.Operator))
            {
                elOperator = elKey.Operator[0];
                if (elKey.Operator.Length > 1)
                {
                    // Move back index in case we're seeing greater or equal to kind of operators
                    index -= elKey.Operator.Length - 1;
                }
            }
            else
            {
                IgnoreElementWhiteSpace(code, ref index);
                elOperator = GetElementCharacter(code, index);
            }

            if (!Common.Constants.Scripts.Operators.Any(p => p == elOperator))
            {
                if (Common.Constants.Scripts.InlineOperators.Any(p => p.Equals(elKey.Value, StringComparison.OrdinalIgnoreCase)))
                {
                    IgnoreElementWhiteSpace(code, ref index);
                    var inlineValues = GetElementValue(code, ref index);
                    if (inlineValues.Terminator.HasValue)
                    {
                        index++;
                        var values = GetElements(code, ref index);
                        var scriptElement = DIResolver.Get<IScriptElement>();
                        scriptElement.Key = elKey.Value;
                        scriptElement.Values = values;
                        return scriptElement;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (elKey.Terminator.GetValueOrDefault() == Common.Constants.Scripts.OpenObject)
                {
                    // Seriously paradox language is way too weird, we need to cover cases such as this aw well: statement = { { code } }
                    index++;
                    var values = GetElements(code, ref index);
                    var scriptElement = DIResolver.Get<IScriptElement>();
                    scriptElement.Key = elKey.Value;
                    scriptElement.Values = values;
                    return scriptElement;
                }
                else if (!string.IsNullOrWhiteSpace(elKey.Value))
                {
                    var scriptElement = DIResolver.Get<IScriptElement>();
                    scriptElement.Key = elKey.Value;
                    scriptElement.IsSimpleType = true;
                    return scriptElement;
                }
                else
                {
                    return null;
                }
            }

            IgnoreElementWhiteSpace(code, ref index);
            var elValue = GetElementValue(code, ref index);
            if (elValue.Terminator.HasValue)
            {
                if (elValue.Terminator.GetValueOrDefault() != Common.Constants.Scripts.OpenObject)
                {
                    return null;
                }

                index++;
                var values = GetElements(code, ref index);
                var scriptElement = DIResolver.Get<IScriptElement>();
                scriptElement.Key = elKey.Value;
                scriptElement.Values = values;
                scriptElement.Operator = Common.Constants.Scripts.EqualsOperator.ToString();
                return scriptElement;
            }
            else if (Common.Constants.Scripts.InlineOperators.Any(p => p.Equals(elValue.Value, StringComparison.OrdinalIgnoreCase)))
            {
                IgnoreElementWhiteSpace(code, ref index);
                var inlineValues = GetElementValue(code, ref index);
                if (inlineValues.Terminator.HasValue)
                {
                    index++;
                    var values = GetElements(code, ref index);
                    var parentElement = DIResolver.Get<IScriptElement>();
                    parentElement.Key = elKey.Value;
                    parentElement.Operator = Common.Constants.Scripts.EqualsOperator.ToString();
                    var childElement = DIResolver.Get<IScriptElement>();
                    childElement.Key = elValue.Value;
                    childElement.Values = values;
                    parentElement.Values = new List<IScriptElement> { childElement };
                    return parentElement;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                var scriptElement = DIResolver.Get<IScriptElement>();
                scriptElement.Key = elKey.Value;
                scriptElement.Value = elValue.Value;
                scriptElement.IsSimpleType = true;
                scriptElement.Operator = elValue.Operator;
                return scriptElement;
            }
        }

        /// <summary>
        /// Gets the element character.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="index">The index.</param>
        /// <returns>System.Nullable&lt;System.Char&gt;.</returns>
        protected char? GetElementCharacter(IList<char> code, int index)
        {
            char? character = null;
            if (index < code.Count)
            {
                character = code[index];
            }

            return character;
        }

        /// <summary>
        /// Gets the elements.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="index">The index.</param>
        /// <returns>IEnumerable&lt;IScriptElement&gt;.</returns>
        /// <exception cref="System.ArgumentException">Unknown script syntax error near code:{sb}</exception>
        protected IEnumerable<IScriptElement> GetElements(List<char> code, ref int index)
        {
            var counter = 0;
            var values = new List<IScriptElement>();
            for (var i = index; i < code.Count; i++)
            {
                var prevIndex = index;
                var character = GetElementCharacter(code, i);
                if (character == null || character == Common.Constants.Scripts.CloseObject)
                {
                    index = i + 1;
                    break;
                }

                var el = GetElement(code, ref i);
                if (el != null)
                {
                    values.Add(el);
                }

                // Move position back by
                i = index = i - 1;
                if (prevIndex >= index)
                {
                    counter++;
                    if (counter >= TraceBackTolerance)
                    {
                        // track back 50 characters and dump the code
                        var dumpIndex = prevIndex - 50;
                        while (true)
                        {
                            if (dumpIndex < 0)
                            {
                                dumpIndex = 0;
                                break;
                            }
                            else if (char.IsWhiteSpace(code[dumpIndex]))
                            {
                                break;
                            }

                            dumpIndex--;
                        }

                        var end = dumpIndex + 200 > code.Count ? code.Count : dumpIndex + 200;
                        var sb = new StringBuilder();
                        for (var dump = dumpIndex; dump < end; dump++)
                        {
                            sb.Append(code[dump]);
                        }

                        throw new ArgumentException($"Unknown script syntax error near code:{sb}");
                    }
                }
                else
                {
                    counter = 0;
                }
            }

            return values;
        }

        /// <summary>
        /// Gets the element value.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="index">The index.</param>
        /// <param name="breakOnOperatorTerminator">if set to <c>true</c> [break on operator terminator].</param>
        /// <returns>ElementValue.</returns>
        protected ElementValue GetElementValue(List<char> code, ref int index, bool breakOnOperatorTerminator = false)
        {
            char? terminator = null;
            var sbValue = new StringBuilder();
            var sbOperator = new StringBuilder();
            var openQuote = false;
            var operatorOpened = false;
            var squareBracketOpen = false;
            int? escapeIndex = null;
            for (var i = index; i < code.Count; i++)
            {
                var character = GetElementCharacter(code, i);
                if (character == null)
                {
                    index = i;
                    break;
                }

                if (character == Common.Constants.Scripts.EscapeCharacter)
                {
                    escapeIndex = i;
                }
                else if (character == Common.Constants.Scripts.Quote)
                {
                    if (!escapeIndex.HasValue)
                    {
                        if (operatorOpened)
                        {
                            if (breakOnOperatorTerminator)
                            {
                                break;
                            }
                            else
                            {
                                operatorOpened = false;
                            }
                        }

                        if (openQuote)
                        {
                            sbValue.Append(character.GetValueOrDefault());
                            index = i + 1;
                            break;
                        }

                        openQuote = true;
                    }
                    else
                    {
                        escapeIndex = null;
                    }
                }
                else if (character == Common.Constants.Scripts.SquareOpenBracket)
                {
                    squareBracketOpen = true;
                }
                else if (character == Common.Constants.Scripts.SquareCloseBracket)
                {
                    squareBracketOpen = false;
                }
                else if (Common.Constants.Scripts.Operators.Any(p => p == character.GetValueOrDefault()))
                {
                    if (!openQuote && !squareBracketOpen)
                    {
                        operatorOpened = true;
                    }
                }
                else if (char.IsWhiteSpace(character.GetValueOrDefault()) && !openQuote && !squareBracketOpen)
                {
                    if (!operatorOpened)
                    {
                        index = i;
                        break;
                    }
                    else
                    {
                        if (breakOnOperatorTerminator)
                        {
                            break;
                        }
                        else
                        {
                            sbOperator.Append(character.GetValueOrDefault());
                            IgnoreElementWhiteSpace(code, ref i);

                            // Move back by 1
                            i--;
                            operatorOpened = false;
                            index = i;
                            continue;
                        }
                    }
                }
                else if (Common.Constants.Scripts.CodeTerminators.Any(p => p == character.GetValueOrDefault()) && !openQuote)
                {
                    terminator = character;
                    index = i;
                    break;
                }
                else if (!char.IsWhiteSpace(character.GetValueOrDefault()))
                {
                    if (operatorOpened)
                    {
                        if (breakOnOperatorTerminator)
                        {
                            break;
                        }
                        else
                        {
                            operatorOpened = false;
                        }
                    }
                }

                if (!operatorOpened)
                {
                    sbValue.Append(character.GetValueOrDefault());
                }
                else
                {
                    sbOperator.Append(character.GetValueOrDefault());
                }

                index = i;
                if (escapeIndex.HasValue && index - escapeIndex.GetValueOrDefault() >= 1)
                {
                    escapeIndex = null;
                }
            }

            return new ElementValue { Value = sbValue.ToString(), Operator = sbOperator.ToString().Trim(), Terminator = terminator };
        }

        /// <summary>
        /// Ignores the element white space.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="index">The index.</param>
        protected void IgnoreElementWhiteSpace(List<char> code, ref int index)
        {
            for (var i = index; i < code.Count; i++)
            {
                var character = GetElementCharacter(code, i);
                if (character == null || char.IsWhiteSpace(character.GetValueOrDefault()))
                {
                    // Nothing
                }
                else
                {
                    index = i;
                    break;
                }
            }
        }

        /// <summary>
        /// Parses the elements.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="lines">The lines.</param>
        /// <returns>IEnumerable&lt;IScriptElement&gt;.</returns>
        protected IEnumerable<IScriptElement> ParseElements(string file, IEnumerable<string> lines)
        {
            var result = new List<IScriptElement>();
            var validCodeLines = CleanCode(file, lines);
            var code = string.Join(Environment.NewLine, validCodeLines).ToList();
            for (var i = 0; i < code.Count; i++)
            {
                var element = GetElement(code, ref i);
                if (element != null)
                {
                    result.Add(element);
                }
            }

            return result;
        }

        /// <summary>
        /// Parses the script data.
        /// </summary>
        /// <param name="lines">The lines.</param>
        /// <param name="file">The file.</param>
        /// <param name="performSimpleCheck">if set to <c>true</c> [perform simple check].</param>
        /// <param name="skipValidityCheck">if set to <c>true</c> [skip validity check].</param>
        /// <returns>IParseResponse.</returns>
        protected IParseResponse ParseScriptData(IEnumerable<string> lines, string file = Constants.EmptyParam, bool performSimpleCheck = false, bool skipValidityCheck = false)
        {
            var result = DIResolver.Get<IParseResponse>();
            if (skipValidityCheck)
            {
                result.UseSimpleValidation = null;
                try
                {
                    result.Values = ParseElements(file, lines);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    var parseError = DIResolver.Get<IScriptError>();
                    parseError.Message = ex.Message;
                    result.Error = parseError;
                }
            }
            else
            {
                result.UseSimpleValidation = performSimpleCheck;
                var error = PerformValidityCheck(lines, file, performSimpleCheck);
                if (error != null)
                {
                    result.Error = error;
                }
                else
                {
                    try
                    {
                        result.Values = ParseElements(file, lines);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                        var parseError = DIResolver.Get<IScriptError>();
                        parseError.Message = ex.Message;
                        result.Error = parseError;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Performs the basic validity check.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="lines">The lines.</param>
        /// <returns>IScriptError.</returns>
        protected IScriptError PerformBasicValidityCheck(string file, IEnumerable<string> lines)
        {
            lines = CleanCode(file, lines);
            var text = string.Join(Environment.NewLine, lines);
            var openBracket = text.Count(s => s == Common.Constants.Scripts.OpenObject);
            var closeBracket = text.Count(s => s == Common.Constants.Scripts.CloseObject);
            if (openBracket != closeBracket)
            {
                var error = DIResolver.Get<IScriptError>();
                error.Message = "Number of open and close curly brackets does not match. This indicates a syntax error somewhere in the file.";
                return error;
            }

            return null;
        }

        /// <summary>
        /// Cleans the comments.
        /// </summary>
        /// <param name="commentId">The comment identifier.</param>
        /// <param name="line">The line.</param>
        /// <returns>System.String.</returns>
        protected string RemoveInlineComments(string commentId, string line)
        {
            if (line.IndexOf(commentId, StringComparison.Ordinal) > 0)
            {
                var sb = new StringBuilder();
                var split = line.Split(commentId);
                var counter = 0;
                var count = split.Length;
                var quoteCount = 0;
                foreach (var item in split)
                {
                    counter++;
                    var previousQuoteCount = quoteCount;
                    quoteCount += item.Count(p => p == Common.Constants.Scripts.Quote);
                    if (counter == 1)
                    {
                        sb.Append(item);
                    }
                    else
                    {
                        var quoteIndex = item.IndexOf(Common.Constants.Scripts.Quote);
                        if (quoteIndex > -1 && quoteCount == 2 && previousQuoteCount > 0)
                        {
                            sb.Append($"#{item[..(quoteIndex + 1)]}");
                            break;
                        }
                        else if (counter < count && previousQuoteCount > 0)
                        {
                            sb.Append($"#{item}");
                        }
                    }
                }

                return sb.ToString().Trim(commentId);
            }

            return line;
        }

        #endregion Methods

        #region Classes

        /// <summary>
        /// Class ElementValue.
        /// </summary>
        protected class ElementValue
        {
            #region Fields

            /// <summary>
            /// The value
            /// </summary>
            private string value = string.Empty;

            #endregion Fields

            #region Properties

            /// <summary>
            /// Gets or sets the operator.
            /// </summary>
            /// <value>The operator.</value>
            public string Operator { get; set; }

            /// <summary>
            /// Gets or sets the terminator.
            /// </summary>
            /// <value>The terminator.</value>
            public char? Terminator { get; set; }

            /// <summary>
            /// Gets or sets the value.
            /// </summary>
            /// <value>The value.</value>
            public string Value
            {
                get
                {
                    return value;
                }
                set
                {
                    var val = value ?? string.Empty;
                    var containsComma = false;
#pragma warning disable CA1866
                    if (val.EndsWith(","))
#pragma warning restore CA1866
                    {
                        val = val.Trim(",");
                        containsComma = true;
                    }

                    this.value = decimal.TryParse(val, NumberStyles.Number, CultureInfo.InvariantCulture, out var result) ? result.ToString("G0", CultureInfo.InvariantCulture) : val;
                    if (containsComma)
                    {
                        this.value += ",";
                    }
                }
            }

            #endregion Properties
        }

        #endregion Classes
    }
}
