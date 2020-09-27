﻿// ***********************************************************************
// Assembly         : IronyModManager.Models.Common
// Author           : Mario
// Created          : 02-12-2020
//
// Last Modified By : Mario
// Last Modified On : 09-26-2020
// ***********************************************************************
// <copyright file="IGame.cs" company="Mario">
//     Mario
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Collections.Generic;
using System;
using IronyModManager.Localization;

namespace IronyModManager.Models.Common
{
    /// <summary>
    /// Interface IGame
    /// Implements the <see cref="IronyModManager.Models.Common.IModel" />
    /// Implements the <see cref="IronyModManager.Localization.ILocalizableModel" />
    /// </summary>
    /// <seealso cref="IronyModManager.Models.Common.IModel" />
    /// <seealso cref="IronyModManager.Localization.ILocalizableModel" />
    public interface IGame : IModel, ILocalizableModel
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether [advanced features supported].
        /// </summary>
        /// <value><c>true</c> if [advanced features supported]; otherwise, <c>false</c>.</value>
        bool AdvancedFeaturesSupported { get; set; }

        /// <summary>
        /// Gets or sets the base game directory.
        /// </summary>
        /// <value>The base game directory.</value>
        string BaseGameDirectory { get; set; }

        /// <summary>
        /// Gets or sets the checksum folders.
        /// </summary>
        /// <value>The checksum folders.</value>
        IEnumerable<string> ChecksumFolders { get; set; }

        /// <summary>
        /// Gets or sets the executable location.
        /// </summary>
        /// <value>The executable location.</value>
        string ExecutableLocation { get; set; }

        /// <summary>
        /// Gets or sets the game folders.
        /// </summary>
        /// <value>The game folders.</value>
        IEnumerable<string> GameFolders { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is selected.
        /// </summary>
        /// <value><c>true</c> if this instance is selected; otherwise, <c>false</c>.</value>
        bool IsSelected { get; set; }

        /// <summary>
        /// Gets or sets the launch arguments.
        /// </summary>
        /// <value>The launch arguments.</value>
        string LaunchArguments { get; set; }

        /// <summary>
        /// Gets or sets the name of the launcher settings file.
        /// </summary>
        /// <value>The name of the launcher settings file.</value>
        string LauncherSettingsFileName { get; set; }

        /// <summary>
        /// Gets or sets the launcher settings prefix.
        /// </summary>
        /// <value>The launcher settings prefix.</value>
        string LauncherSettingsPrefix { get; set; }

        /// <summary>
        /// Gets or sets the log location.
        /// </summary>
        /// <value>The log location.</value>
        string LogLocation { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [refresh descriptors].
        /// </summary>
        /// <value><c>true</c> if [refresh descriptors]; otherwise, <c>false</c>.</value>
        bool RefreshDescriptors { get; set; }

        /// <summary>
        /// Gets or sets the remote steam user directory.
        /// </summary>
        /// <value>The remote steam user directory.</value>
        IEnumerable<string> RemoteSteamUserDirectory { get; set; }

        /// <summary>
        /// Gets or sets the steam application identifier.
        /// </summary>
        /// <value>The steam application identifier.</value>
        int SteamAppId { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        string Type { get; set; }

        /// <summary>
        /// Gets or sets the user directory.
        /// </summary>
        /// <value>The user directory.</value>
        string UserDirectory { get; set; }

        /// <summary>
        /// Gets or sets the workshop directory.
        /// </summary>
        /// <value>The workshop directory.</value>
        string WorkshopDirectory { get; set; }

        #endregion Properties
    }
}
