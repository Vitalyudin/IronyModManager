﻿// ***********************************************************************
// Assembly         : IronyModManager.Services
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

namespace IronyModManager.Services
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
            CreateMap<ITheme, IPreferences>()
                .ForMember(p => p.Theme, o => o.MapFrom(m => m.Type)).ReverseMap().IgnoreAllUnmappedMembers();
            CreateMap<ILanguage, IPreferences>()
                .ForMember(p => p.Locale, o => o.MapFrom(m => m.Abrv)).ReverseMap().IgnoreAllUnmappedMembers();
            CreateMap<IGame, IPreferences>()
                .ForMember(m => m.Game, o => o.MapFrom(s => s.Type)).ReverseMap().IgnoreAllUnmappedMembers();
            CreateMap<IUpdateSettings, IPreferences>()
                .ForMember(m => m.AutoUpdates, o => o.MapFrom(s => s.AutoUpdates))
                .ForMember(m => m.CheckForPrerelease, o => o.MapFrom(s => s.CheckForPrerelease))
                .ForMember(m => m.LastSkippedVersion, o => o.MapFrom(s => s.LastSkippedVersion))
                .ReverseMap()
                .IgnoreAllUnmappedMembers();
            CreateMap<IExternalEditor, IPreferences>()
                .ForMember(m => m.ExternalEditorLocation, o => o.MapFrom(s => s.ExternalEditorLocation))
                .ForMember(m => m.ExternalEditorParameters, o => o.MapFrom(s => s.ExternalEditorParameters))
                .ReverseMap()
                .IgnoreAllUnmappedMembers();
            CreateMap<IDefinition, IDefinition>().ReverseMap();
            CreateMap<IMod, IMod>().ReverseMap();
            CreateMap<INotificationPosition, IPreferences>()
                .ForMember(p => p.NotificationPosition, o => o.MapFrom(m => m.Type)).ReverseMap().IgnoreAllUnmappedMembers();
            CreateMap<IPromptNotifications, IPreferences>()
                .ForMember(m => m.ConflictSolverPromptShown, o => o.MapFrom(s => s.ConflictSolverPromptShown))
                .ReverseMap()
                .IgnoreAllUnmappedMembers();
            CreateMap<IPreferences, IConflictSolverColors>().ForMember(m => m.ConflictSolverDeletedLineColor, o => o.MapFrom(s => s.ConflictSolverDeletedLineColor))
                .ForMember(m => m.ConflictSolverImaginaryLineColor, o => o.MapFrom(s => s.ConflictSolverImaginaryLineColor)).ForMember(m => m.ConflictSolverInsertedLineColor, o => o.MapFrom(s => s.ConflictSolverInsertedLineColor))
                .ForMember(m => m.ConflictSolverModifiedLineColor, o => o.MapFrom(s => s.ConflictSolverModifiedLineColor)).ReverseMap().IgnoreAllUnmappedMembers();
        }

        #endregion Constructors
    }
}
