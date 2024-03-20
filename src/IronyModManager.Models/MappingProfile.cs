﻿// ***********************************************************************
// Assembly         : IronyModManager.Models
// Author           : Mario
// Created          : 01-11-2020
//
// Last Modified By : Mario
// Last Modified On : 03-18-2024
// ***********************************************************************
// <copyright file="MappingProfile.cs" company="Mario">
//     Copyright (c) Mario. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using IronyModManager.Models.Common;
using IronyModManager.Shared;
using IronyModManager.Shared.Models;

namespace IronyModManager.Models
{
    /// <summary>
    /// Class MappingProfile.
    /// Implements the <see cref="IronyModManager.Shared.BaseMappingProfile" />
    /// </summary>
    /// <seealso cref="IronyModManager.Shared.BaseMappingProfile" />
    [ExcludeFromCoverage("Mapping profile shouldn't be tested.")]
    public class MappingProfile : BaseMappingProfile
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingProfile" /> class.
        /// </summary>
        public MappingProfile()
        {
            CreateMap<IPreferences, Preferences>().ReverseMap();
            CreateMap<ITheme, Theme>().ReverseMap();
            CreateMap<ILanguage, Language>().ReverseMap();
            CreateMap<IWindowState, WindowState>().ReverseMap();
            CreateMap<IGame, Game>().ReverseMap();
            CreateMap<IMod, Mod>().ReverseMap();
            CreateMap<IModObject, IMod>().ReverseMap();
            CreateMap<IModObject, Mod>().ReverseMap();
            CreateMap<IAppState, AppState>().ReverseMap();
            CreateMap<IModCollection, ModCollection>().ReverseMap();
            CreateMap<IConflictResult, ConflictResult>().ReverseMap();
            CreateMap<IPriorityDefinitionResult, PriorityDefinitionResult>().ReverseMap();
            CreateMap<IGameSettings, GameSettings>().ReverseMap();
            CreateMap<IUpdateSettings, UpdateSettings>().ReverseMap();
            CreateMap<IHashFileReport, HashFileReport>().ReverseMap();
            CreateMap<IHashReport, HashReport>().ReverseMap();
            CreateMap<IExternalEditor, ExternalEditor>().ReverseMap();
            CreateMap<IModInstallationResult, ModInstallationResult>().ReverseMap();
            CreateMap<IPermissionCheckResult, PermissionCheckResult>().ReverseMap();
            CreateMap<IDLC, DLC>().ReverseMap();
            CreateMap<IDLCObject, IDLC>().ReverseMap();
            CreateMap<IDLCObject, DLC>().ReverseMap();
            CreateMap<INotificationPosition, NotificationPosition>().ReverseMap();
            CreateMap<IPromptNotifications, PromptNotifications>().ReverseMap();
            CreateMap<IModCollectionSourceInfo, ModCollectionSourceInfo>().ReverseMap();
            CreateMap<IModIgnoreConfiguration, ModIgnoreConfiguration>().ReverseMap();
            CreateMap<IGameLanguage, GameLanguage>().ReverseMap();
            CreateMap<IConflictSolverColors, ConflictSolverColors>().ReverseMap();
        }

        #endregion Constructors
    }
}
