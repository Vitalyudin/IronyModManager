﻿// ***********************************************************************
// Assembly         : IronyModManager.Services
// Author           : Mario
// Created          : 05-14-2023
//
// Last Modified By : Mario
// Last Modified On : 10-17-2024
// ***********************************************************************
// <copyright file="ObjectClone.cs" company="Mario">
//     Mario
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using IronyModManager.DI;
using IronyModManager.Shared;
using IronyModManager.Shared.Models;

namespace IronyModManager.Services
{
    /// <summary>
    /// Class ObjectClone.
    /// Implements the <see cref="IObjectClone" />
    /// </summary>
    /// <seealso cref="IObjectClone" />
    public class ObjectClone : IObjectClone
    {
        #region Methods

        /// <summary>
        /// Clones the definition.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="includeCode">if set to <c>true</c> [include code].</param>
        /// <returns>IDefinition.</returns>
        public IDefinition CloneDefinition(IDefinition definition, bool includeCode)
        {
            var newDefinition = DIResolver.Get<IDefinition>();
            if (includeCode)
            {
                newDefinition.Code = definition.Code;
            }

            newDefinition.ContentSHA = definition.ContentSHA;
            newDefinition.DefinitionSHA = definition.DefinitionSHA;
            newDefinition.Dependencies = definition.Dependencies;
            newDefinition.ErrorColumn = definition.ErrorColumn;
            newDefinition.ErrorLine = definition.ErrorLine;
            newDefinition.ErrorMessage = definition.ErrorMessage;
            newDefinition.File = definition.File;
            newDefinition.GeneratedFileNames = definition.GeneratedFileNames;
            newDefinition.OverwrittenFileNames = definition.OverwrittenFileNames;
            newDefinition.AdditionalFileNames = definition.AdditionalFileNames;
            newDefinition.Id = definition.Id;
            newDefinition.ModName = definition.ModName;
            newDefinition.Type = definition.Type;
            newDefinition.UsedParser = definition.UsedParser;
            newDefinition.ValueType = definition.ValueType;
            newDefinition.Tags = definition.Tags;
            newDefinition.OriginalCode = definition.OriginalCode;
            newDefinition.CodeSeparator = definition.CodeSeparator;
            newDefinition.CodeTag = definition.CodeTag;
            newDefinition.Order = definition.Order;
            newDefinition.OriginalModName = definition.OriginalModName;
            newDefinition.OriginalFileName = definition.OriginalFileName;
            newDefinition.DiskFile = definition.DiskFile;
            newDefinition.Variables = definition.Variables;
            newDefinition.ExistsInLastFile = definition.ExistsInLastFile;
            newDefinition.VirtualPath = definition.VirtualPath;
            newDefinition.CustomPriorityOrder = definition.CustomPriorityOrder;
            newDefinition.IsCustomPatch = definition.IsCustomPatch;
            newDefinition.IsFromGame = definition.IsFromGame;
            newDefinition.AllowDuplicate = definition.AllowDuplicate;
            newDefinition.ResetType = definition.ResetType;
            newDefinition.FileNameSuffix = definition.FileNameSuffix;
            newDefinition.IsPlaceholder = definition.IsPlaceholder;
            newDefinition.LastModified = definition.LastModified;
            newDefinition.OriginalId = definition.OriginalId;
            newDefinition.UseSimpleValidation = definition.UseSimpleValidation;
            newDefinition.IsSpecialFolder = definition.IsSpecialFolder;
            newDefinition.MergeType = definition.MergeType;
            return newDefinition;
        }

        #endregion Methods
    }
}
