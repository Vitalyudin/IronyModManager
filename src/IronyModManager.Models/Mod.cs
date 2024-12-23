﻿// ***********************************************************************
// Assembly         : IronyModManager.Models
// Author           : Mario
// Created          : 02-29-2020
//
// Last Modified By : Mario
// Last Modified On : 12-23-2024
// ***********************************************************************
// <copyright file="Mod.cs" company="Mario">
//     Mario
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IronyModManager.Models.Common;
using IronyModManager.Shared;

namespace IronyModManager.Models
{
    /// <summary>
    /// Class Mod.
    /// Implements the <see cref="IronyModManager.Models.Common.BaseModel" />
    /// Implements the <see cref="IronyModManager.Models.Common.IMod" />
    /// </summary>
    /// <seealso cref="IronyModManager.Models.Common.BaseModel" />
    /// <seealso cref="IronyModManager.Models.Common.IMod" />
    public class Mod : BaseModel, IMod
    {
        #region Fields

        /// <summary>
        /// The version
        /// </summary>
        private string version;

        /// <summary>
        /// The version data
        /// </summary>
        private Shared.Version versionData;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets or sets the achievement status.
        /// </summary>
        /// <value>The achievement status.</value>
        public virtual AchievementStatus AchievementStatus { get; set; }

        /// <summary>
        /// Gets or sets the additional data.
        /// </summary>
        /// <value>The additional data.</value>
        public virtual IDictionary<string, object> AdditionalData { get; set; }

        /// <summary>
        /// Gets or sets the dependencies.
        /// </summary>
        /// <value>The dependencies.</value>
        [DescriptorProperty("dependencies")]
        public virtual IEnumerable<string> Dependencies { get; set; }

        /// <summary>
        /// Gets or sets the descriptor file.
        /// </summary>
        /// <value>The descriptor file.</value>
        public virtual string DescriptorFile { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        [DescriptorProperty("path", "archive", ".zip", ".bin")]
        public virtual string FileName { get; set; }

        /// <summary>
        /// Gets or sets the files.
        /// </summary>
        /// <value>The files.</value>
        public virtual IEnumerable<string> Files { get; set; }

        /// <summary>
        /// Gets or sets the full path.
        /// </summary>
        /// <value>The full path.</value>
        public virtual string FullPath { get; set; }

        /// <summary>
        /// Gets or sets the game.
        /// </summary>
        /// <value>The game.</value>
        public virtual string Game { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is locked.
        /// </summary>
        /// <value><c>true</c> if this instance is locked; otherwise, <c>false</c>.</value>
        public virtual bool IsLocked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is selected.
        /// </summary>
        /// <value><c>true</c> if this instance is selected; otherwise, <c>false</c>.</value>
        public virtual bool IsSelected { get; set; }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        public virtual bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public virtual string JsonId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [DescriptorProperty("name")]
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>The order.</value>
        public virtual int Order { get; set; }

        /// <summary>
        /// Gets the parent directory.
        /// </summary>
        /// <value>The parent directory.</value>
        public virtual string ParentDirectory
        {
            get
            {
                if (string.IsNullOrWhiteSpace(FullPath))
                {
                    return FullPath;
                }

                if (!string.IsNullOrWhiteSpace(Path.GetExtension(FullPath)))
                {
                    return Path.GetDirectoryName(FullPath);
                }

                return FullPath;
            }
        }

        /// <summary>
        /// Gets or sets the picture.
        /// </summary>
        /// <value>The picture.</value>
        [DescriptorProperty("picture")]
        public virtual string Picture { get; set; }

        /// <summary>
        /// Gets or sets the relationship data.
        /// </summary>
        /// <value>The relationship data.</value>
        public virtual IEnumerable<IDictionary<string, object>> RelationshipData { get; set; }

        /// <summary>
        /// Gets or sets the remote identifier.
        /// </summary>
        /// <value>The remote identifier.</value>
        [DescriptorProperty("remote_file_id")]
        public virtual long? RemoteId { get; set; }

        /// <summary>
        /// Gets or sets the replacement path.
        /// </summary>
        /// <value>The replacement path.</value>
        [DescriptorProperty("replace_path", true)]
        public virtual IEnumerable<string> ReplacePath { get; set; }

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>The source.</value>
        public virtual ModSource Source { get; set; }

        /// <summary>
        /// Gets or sets the tags.
        /// </summary>
        /// <value>The tags.</value>
        [DescriptorProperty("tags")]
        public virtual IEnumerable<string> Tags { get; set; }

        /// <summary>
        /// Gets or sets the user dir.
        /// </summary>
        /// <value>The user dir.</value>
        [DescriptorProperty(Constants.DescriptorUserDir, true)]
        public virtual IEnumerable<string> UserDir { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>The version.</value>
        [DescriptorProperty("supported_version")]
        public virtual string Version
        {
            get
            {
                return version;
            }
            set
            {
                versionData = null;
                version = value;
            }
        }

        /// <summary>
        /// Gets or sets the version data.
        /// </summary>
        /// <value>The version data.</value>
        public virtual Shared.Version VersionData
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Version))
                {
                    var convertedVersion = Version.ToVersion();
                    if (convertedVersion != null)
                    {
                        versionData = convertedVersion;
                    }
                    else
                    {
                        versionData = new Shared.Version();
                    }
                }
                else
                {
                    versionData = new Shared.Version();
                }

                return versionData;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Determines whether the specified term is match.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <returns><c>true</c> if the specified term is match; otherwise, <c>false</c>.</returns>
        public virtual bool IsMatch(string term)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return false;
            }

            term ??= string.Empty;
            return Name.StartsWith(term, StringComparison.OrdinalIgnoreCase);
        }

        #endregion Methods
    }
}
