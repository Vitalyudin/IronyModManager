﻿// ***********************************************************************
// Assembly         : IronyModManager.Services.Tests
// Author           : Mario
// Created          : 06-19-2020
//
// Last Modified By : Mario
// Last Modified On : 03-19-2024
// ***********************************************************************
// <copyright file="ModMergeServiceTests.cs" company="Mario">
//     Mario
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using IronyModManager.IO;
using IronyModManager.IO.Common;
using IronyModManager.IO.Common.Mods;
using IronyModManager.IO.Common.Readers;
using IronyModManager.Models;
using IronyModManager.Models.Common;
using IronyModManager.Parser.Common;
using IronyModManager.Parser.Common.Mod;
using IronyModManager.Parser.Mod;
using IronyModManager.Services.Common;
using IronyModManager.Shared.Cache;
using IronyModManager.Shared.Configuration;
using IronyModManager.Shared.MessageBus;
using IronyModManager.Shared.Models;
using IronyModManager.Storage.Common;
using IronyModManager.Tests.Common;
using Moq;
using Xunit;

namespace IronyModManager.Services.Tests
{
    /// <summary>
    /// Class ModMergeServiceTests.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0028:Simplify collection initialization", Justification = "Old unit test, forget it")]
    public class ModMergeServiceTests
    {
        /// <summary>
        /// Defines the test method Should_not_create_file_merge_mod_due_to_no_game_set.
        /// </summary>
        [Fact]
        public async Task Should_not_create_file_merge_mod_due_to_no_game_set()
        {
            var messageBus = new Mock<IMessageBus>();
            messageBus.Setup(p => p.PublishAsync(It.IsAny<IMessageBusEvent>()));
            messageBus.Setup(p => p.Publish(It.IsAny<IMessageBusEvent>()));
            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            var modMergeExporter = new Mock<IModMergeExporter>();
            var infoProvider = new Mock<IDefinitionInfoProvider>();
            gameService.Setup(p => p.GetSelected()).Returns((IGame)null);

            var service = new ModMergeService(null, null, null, new Cache(), messageBus.Object, modPatchExporter.Object, modMergeExporter.Object,
                new List<IDefinitionInfoProvider> { infoProvider.Object }, reader.Object, modWriter.Object,
                modParser.Object, gameService.Object, storageProvider.Object, mapper.Object);

            var result = await service.MergeCollectionByFilesAsync("test");

            result.Should().BeNull();
        }

        /// <summary>
        /// Defines the test method Should_not_create_file_merge_mod_due_to_no_collection_name.
        /// </summary>
        [Fact]
        public async Task Should_not_create_file_merge_mod_due_to_no_collection_name()
        {
            var messageBus = new Mock<IMessageBus>();
            messageBus.Setup(p => p.PublishAsync(It.IsAny<IMessageBusEvent>()));
            messageBus.Setup(p => p.Publish(It.IsAny<IMessageBusEvent>()));
            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            var modMergeExporter = new Mock<IModMergeExporter>();
            var infoProvider = new Mock<IDefinitionInfoProvider>();
            gameService.Setup(p => p.GetSelected()).Returns(new Game { Type = "Should_not_create_file_merge_mod_due_to_no_collection_name", UserDirectory = "C:\\Users\\Fake", WorkshopDirectory = new List<string> { "C:\\fake" } });

            var service = new ModMergeService(null, null, null, new Cache(), messageBus.Object, modPatchExporter.Object, modMergeExporter.Object,
                new List<IDefinitionInfoProvider> { infoProvider.Object }, reader.Object, modWriter.Object,
                modParser.Object, gameService.Object, storageProvider.Object, mapper.Object);

            var result = await service.MergeCollectionByFilesAsync(string.Empty);

            result.Should().BeNull();
        }

        /// <summary>
        /// Defines the test method Should_create_file_merge_mod.
        /// </summary>
        [Fact]
        public async Task Should_create_file_merge_mod()
        {
            var messageBus = new Mock<IMessageBus>();
            messageBus.Setup(p => p.PublishAsync(It.IsAny<IMessageBusEvent>()));
            messageBus.Setup(p => p.Publish(It.IsAny<IMessageBusEvent>()));
            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            var modMergeExporter = new Mock<IModMergeExporter>();
            var infoProvider = new Mock<IDefinitionInfoProvider>();

            modMergeExporter.Setup(p => p.ExportFilesAsync(It.IsAny<ModMergeFileExporterParameters>())).Returns(Task.FromResult(true));
            gameService.Setup(p => p.GetSelected()).Returns(new Game { Type = "Should_create_file_merge_mod", UserDirectory = "C:\\Users\\Fake", WorkshopDirectory = new List<string> { "C:\\fake" }, CustomModDirectory = string.Empty });
            var collections = new List<IModCollection> { new ModCollection { IsSelected = true, Mods = new List<string> { "mod/fakemod.mod" }, Name = "test", Game = "Should_create_file_merge_mod" } };
            storageProvider.Setup(s => s.GetModCollections()).Returns(() =>
            {
                return collections;
            });
            var fileInfos = new List<IFileInfo> { new FileInfo { Content = new List<string> { "a" }, FileName = "fakemod.mod", IsBinary = false } };
            reader.Setup(s => s.Read(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<bool>())).Returns(fileInfos);
            modParser.Setup(s => s.Parse(It.IsAny<IEnumerable<string>>(), It.IsAny<DescriptorModType>())).Returns((IEnumerable<string> values, DescriptorModType t) =>
            {
                return new ModObject { FileName = values.First(), Name = values.First() };
            });
            mapper.Setup(s => s.Map<IMod>(It.IsAny<IModObject>())).Returns((IModObject o) =>
            {
                return new Mod { FileName = o.FileName, Name = o.Name };
            });
            modWriter.Setup(p => p.ModDirectoryExists(It.IsAny<ModWriterParameters>())).Returns((ModWriterParameters p) =>
            {
                return false;
            });

            var service = new ModMergeService(null, null, null, new Cache(), messageBus.Object, modPatchExporter.Object, modMergeExporter.Object,
                new List<IDefinitionInfoProvider> { infoProvider.Object }, reader.Object, modWriter.Object,
                modParser.Object, gameService.Object, storageProvider.Object, mapper.Object);

            var result = await service.MergeCollectionByFilesAsync("test");

            result.Should().NotBeNull();
        }

        /// <summary>
        /// Defines the test method Should_not_create_merge_compress_mods_due_to_no_game_set.
        /// </summary>
        [Fact]
        public async Task Should_not_create_merge_compress_mods_due_to_no_game_set()
        {
            var messageBus = new Mock<IMessageBus>();
            messageBus.Setup(p => p.PublishAsync(It.IsAny<IMessageBusEvent>()));
            messageBus.Setup(p => p.Publish(It.IsAny<IMessageBusEvent>()));
            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            var modMergeExporter = new Mock<IModMergeExporter>();
            var infoProvider = new Mock<IDefinitionInfoProvider>();
            gameService.Setup(p => p.GetSelected()).Returns((IGame)null);

            var service = new ModMergeService(null, null, null, new Cache(), messageBus.Object, modPatchExporter.Object, modMergeExporter.Object,
                new List<IDefinitionInfoProvider> { infoProvider.Object }, reader.Object, modWriter.Object,
                modParser.Object, gameService.Object, storageProvider.Object, mapper.Object);

            var result = await service.MergeCompressCollectionAsync("test", "test");

            result.Should().BeNull();
        }

        /// <summary>
        /// Defines the test method Should_not_create_merge_compress_mods_due_to_no_collection_name.
        /// </summary>
        [Fact]
        public async Task Should_not_create_merge_compress_mods_due_to_no_collection_name()
        {
            var messageBus = new Mock<IMessageBus>();
            messageBus.Setup(p => p.PublishAsync(It.IsAny<IMessageBusEvent>()));
            messageBus.Setup(p => p.Publish(It.IsAny<IMessageBusEvent>()));
            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            var modMergeExporter = new Mock<IModMergeExporter>();
            var infoProvider = new Mock<IDefinitionInfoProvider>();
            gameService.Setup(p => p.GetSelected()).Returns(new Game { Type = "Should_not_create_file_merge_mod_due_to_no_collection_name", UserDirectory = "C:\\Users\\Fake", WorkshopDirectory = new List<string> { "C:\\fake" } });

            var service = new ModMergeService(null, null, null, new Cache(), messageBus.Object, modPatchExporter.Object, modMergeExporter.Object,
                new List<IDefinitionInfoProvider> { infoProvider.Object }, reader.Object, modWriter.Object,
                modParser.Object, gameService.Object, storageProvider.Object, mapper.Object);

            var result = await service.MergeCompressCollectionAsync(string.Empty, "test");

            result.Should().BeNull();
        }

        /// <summary>
        /// Defines the test method Should_create_merge_compress_mods.
        /// </summary>
        [Fact]
        public async Task Should_create_merge_compress_mods()
        {
            DISetup.SetupContainer();

            var messageBus = new Mock<IMessageBus>();
            messageBus.Setup(p => p.PublishAsync(It.IsAny<IMessageBusEvent>()));
            messageBus.Setup(p => p.Publish(It.IsAny<IMessageBusEvent>()));
            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            var modMergeExporter = new Mock<IModMergeExporter>();
            var infoProvider = new Mock<IDefinitionInfoProvider>();
            var compressExporter = new Mock<IModMergeCompressExporter>();
            var preferencesService = new Mock<IPreferencesService>();
            preferencesService.Setup(p => p.Get()).Returns(() =>
                new Preferences { MergeCollectionModNameTemplate = null });
            var isValid = false;

            compressExporter.Setup(p => p.Start()).Returns(1);
            compressExporter.Setup(p => p.AddFile(It.IsAny<ModMergeCompressExporterParameters>())).Callback((ModMergeCompressExporterParameters p) =>
            {
                if (p.QueueId.Equals(1) && p.FileName.Equals("descriptor.mod"))
                {
                    isValid = true;
                }
            });
            compressExporter.Setup(p => p.Finalize(It.IsAny<long>(), It.IsAny<string>())).Returns(true);
            gameService.Setup(p => p.GetSelected()).Returns(new Game { Type = "Should_create_file_merge_mod", UserDirectory = "C:\\Users\\Fake", WorkshopDirectory = new List<string> { "C:\\fake" }, CustomModDirectory = string.Empty });
            var collections = new List<IModCollection> { new ModCollection { IsSelected = true, Mods = new List<string> { "mod/fakemod.mod" }, Name = "test", Game = "Should_create_file_merge_mod" } };
            storageProvider.Setup(s => s.GetModCollections()).Returns(() =>
            {
                return collections;
            });
            var fileInfos = new List<IFileInfo> { new FileInfo { Content = new List<string> { "a" }, FileName = "fakemod.mod", IsBinary = false } };
            reader.Setup(s => s.Read(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<bool>())).Returns(fileInfos);
            modParser.Setup(s => s.Parse(It.IsAny<IEnumerable<string>>(), It.IsAny<DescriptorModType>())).Returns((IEnumerable<string> values, DescriptorModType t) =>
            {
                return new ModObject { FileName = values.First(), Name = values.First() };
            });
            mapper.Setup(s => s.Map<IMod>(It.IsAny<IModObject>())).Returns((IModObject o) =>
            {
                return new Mod { FileName = o.FileName, Name = o.Name };
            });
            modWriter.Setup(p => p.ModDirectoryExists(It.IsAny<ModWriterParameters>())).Returns((ModWriterParameters p) =>
            {
                return false;
            });
            DISetup.Container.Register<IDomainConfiguration>(() => new DomainConfigDummy(true));


            var service = new ModMergeService(preferencesService.Object, null, compressExporter.Object, new Cache(), messageBus.Object, modPatchExporter.Object, modMergeExporter.Object,
                new List<IDefinitionInfoProvider> { infoProvider.Object }, reader.Object, modWriter.Object,
                modParser.Object, gameService.Object, storageProvider.Object, mapper.Object);

            var result = await service.MergeCompressCollectionAsync("test", "test");

            result.Should().NotBeNull();
            result.Count().Should().Be(1);
            isValid.Should().BeTrue();
        }

        /// <summary>
        /// Shoulds a create merge compress mods with template.
        /// </summary>
        /// <returns>A Task.<see cref="Task"/></returns>
        [Fact]
        public async Task Should_create_merge_compress_mods_with_template()
        {
            DISetup.SetupContainer();

            var messageBus = new Mock<IMessageBus>();
            messageBus.Setup(p => p.PublishAsync(It.IsAny<IMessageBusEvent>()));
            messageBus.Setup(p => p.Publish(It.IsAny<IMessageBusEvent>()));
            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            var modMergeExporter = new Mock<IModMergeExporter>();
            var infoProvider = new Mock<IDefinitionInfoProvider>();
            var compressExporter = new Mock<IModMergeCompressExporter>();
            var preferencesService = new Mock<IPreferencesService>();
            preferencesService.Setup(p => p.Get()).Returns(() =>
                new Preferences { MergeCollectionModNameTemplate = "{Name} test {Merged}" });
            var isValid = false;

            compressExporter.Setup(p => p.Start()).Returns(1);
            compressExporter.Setup(p => p.AddFile(It.IsAny<ModMergeCompressExporterParameters>())).Callback((ModMergeCompressExporterParameters p) =>
            {
                if (p.QueueId.Equals(1) && p.FileName.Equals("descriptor.mod"))
                {
                    isValid = true;
                }
            });
            compressExporter.Setup(p => p.Finalize(It.IsAny<long>(), It.IsAny<string>())).Returns(true);
            gameService.Setup(p => p.GetSelected()).Returns(new Game { Type = "Should_create_file_merge_mod", UserDirectory = "C:\\Users\\Fake", WorkshopDirectory = new List<string> { "C:\\fake" }, CustomModDirectory = string.Empty });
            var collections = new List<IModCollection> { new ModCollection { IsSelected = true, Mods = new List<string> { "mod/fakemod.mod" }, Name = "test", Game = "Should_create_file_merge_mod" } };
            storageProvider.Setup(s => s.GetModCollections()).Returns(() =>
            {
                return collections;
            });
            var fileInfos = new List<IFileInfo> { new FileInfo { Content = new List<string> { "a" }, FileName = "fakemod.mod", IsBinary = false } };
            reader.Setup(s => s.Read(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<bool>())).Returns(fileInfos);
            modParser.Setup(s => s.Parse(It.IsAny<IEnumerable<string>>(), It.IsAny<DescriptorModType>())).Returns((IEnumerable<string> values, DescriptorModType t) =>
            {
                return new ModObject { FileName = values.First(), Name = values.First() };
            });
            mapper.Setup(s => s.Map<IMod>(It.IsAny<IModObject>())).Returns((IModObject o) =>
            {
                return new Mod { FileName = o.FileName, Name = o.Name };
            });
            modWriter.Setup(p => p.ModDirectoryExists(It.IsAny<ModWriterParameters>())).Returns((ModWriterParameters p) =>
            {
                return false;
            });
            DISetup.Container.Register<IDomainConfiguration>(() => new DomainConfigDummy(true));


            var service = new ModMergeService(preferencesService.Object, null, compressExporter.Object, new Cache(), messageBus.Object, modPatchExporter.Object, modMergeExporter.Object,
                new List<IDefinitionInfoProvider> { infoProvider.Object }, reader.Object, modWriter.Object,
                modParser.Object, gameService.Object, storageProvider.Object, mapper.Object);

            var result = await service.MergeCompressCollectionAsync("test", "test");

            result.Should().NotBeNull();
            result.Count().Should().Be(1);
            isValid.Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method Should_not_have_free_space_due_to_no_game_set.
        /// </summary>
        [Fact]
        public async Task Should_not_have_free_space_due_to_no_game_set()
        {
            var messageBus = new Mock<IMessageBus>();
            messageBus.Setup(p => p.PublishAsync(It.IsAny<IMessageBusEvent>()));
            messageBus.Setup(p => p.Publish(It.IsAny<IMessageBusEvent>()));
            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            var modMergeExporter = new Mock<IModMergeExporter>();
            var infoProvider = new Mock<IDefinitionInfoProvider>();
            gameService.Setup(p => p.GetSelected()).Returns((IGame)null);

            var service = new ModMergeService(null, null, null, new Cache(), messageBus.Object, modPatchExporter.Object, modMergeExporter.Object,
                new List<IDefinitionInfoProvider> { infoProvider.Object }, reader.Object, modWriter.Object,
                modParser.Object, gameService.Object, storageProvider.Object, mapper.Object);

            var result = await service.HasEnoughFreeSpaceAsync("test");

            result.Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method Should_not_have_free_space_due_to_no_collection_name.
        /// </summary>
        [Fact]
        public async Task Should_not_have_free_space_due_to_no_collection_name()
        {
            var messageBus = new Mock<IMessageBus>();
            messageBus.Setup(p => p.PublishAsync(It.IsAny<IMessageBusEvent>()));
            messageBus.Setup(p => p.Publish(It.IsAny<IMessageBusEvent>()));
            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            var modMergeExporter = new Mock<IModMergeExporter>();
            var infoProvider = new Mock<IDefinitionInfoProvider>();
            gameService.Setup(p => p.GetSelected()).Returns(new Game
            {
                Type = "Should_not_have_free_space_due_to_no_collection_name",
                UserDirectory = "C:\\Users\\Fake",
                WorkshopDirectory = new List<string> { "C:\\fake" },
                CustomModDirectory = string.Empty
            });

            var service = new ModMergeService(null, null, null, new Cache(), messageBus.Object, modPatchExporter.Object, modMergeExporter.Object,
                new List<IDefinitionInfoProvider> { infoProvider.Object }, reader.Object, modWriter.Object,
                modParser.Object, gameService.Object, storageProvider.Object, mapper.Object);

            var result = await service.HasEnoughFreeSpaceAsync(string.Empty);

            result.Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method Should_not_have_free_space_due_to_no_mods.
        /// </summary>
        [Fact]
        public async Task Should_not_have_free_space_due_to_no_mods()
        {
            var messageBus = new Mock<IMessageBus>();
            messageBus.Setup(p => p.PublishAsync(It.IsAny<IMessageBusEvent>()));
            messageBus.Setup(p => p.Publish(It.IsAny<IMessageBusEvent>()));
            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            var modMergeExporter = new Mock<IModMergeExporter>();
            var infoProvider = new Mock<IDefinitionInfoProvider>();
            gameService.Setup(p => p.GetSelected()).Returns(new Game
            {
                Type = "Should_not_have_free_space_due_to_no_mods",
                UserDirectory = "C:\\Users\\Fake",
                WorkshopDirectory = new List<string> { "C:\\fake" },
                CustomModDirectory = string.Empty
            });

            var service = new ModMergeService(null, null, null, new Cache(), messageBus.Object, modPatchExporter.Object, modMergeExporter.Object,
                new List<IDefinitionInfoProvider> { infoProvider.Object }, reader.Object, modWriter.Object,
                modParser.Object, gameService.Object, storageProvider.Object, mapper.Object);

            var result = await service.HasEnoughFreeSpaceAsync("test");

            result.Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method Should_have_free_space_to_create_merge_mod.
        /// </summary>
        [Fact]
        public async Task Should_have_free_space_to_create_merge_mod()
        {
            DISetup.SetupContainer();

            var messageBus = new Mock<IMessageBus>();
            messageBus.Setup(p => p.PublishAsync(It.IsAny<IMessageBusEvent>()));
            messageBus.Setup(p => p.Publish(It.IsAny<IMessageBusEvent>()));
            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            var modMergeExporter = new Mock<IModMergeExporter>();
            var infoProvider = new Mock<IDefinitionInfoProvider>();
            var diskInfoProvider = new Mock<IDriveInfoProvider>();

            modMergeExporter.Setup(p => p.ExportFilesAsync(It.IsAny<ModMergeFileExporterParameters>())).Returns(Task.FromResult(true));
            gameService.Setup(p => p.GetSelected()).Returns(new Game
            {
                Type = "Should_have_free_space_to_create_merge_mod",
                UserDirectory = "C:\\Users\\Fake",
                WorkshopDirectory = new List<string> { "C:\\fake" },
                CustomModDirectory = string.Empty
            });
            var collections = new List<IModCollection> { new ModCollection { IsSelected = true, Mods = new List<string> { "mod/fakemod.mod" }, Name = "test", Game = "Should_have_free_space_to_create_merge_mod" } };
            storageProvider.Setup(s => s.GetModCollections()).Returns(() =>
            {
                return collections;
            });
            var fileInfos = new List<IFileInfo> { new FileInfo { Content = new List<string> { "a" }, FileName = "fakemod.mod", IsBinary = false } };
            reader.Setup(s => s.Read(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<bool>())).Returns(fileInfos);
            modParser.Setup(s => s.Parse(It.IsAny<IEnumerable<string>>(), It.IsAny<DescriptorModType>())).Returns((IEnumerable<string> values, DescriptorModType t) =>
            {
                return new ModObject { FileName = values.First(), Name = values.First() };
            });
            mapper.Setup(s => s.Map<IMod>(It.IsAny<IModObject>())).Returns((IModObject o) =>
            {
                return new Mod { FileName = o.FileName, Name = o.Name };
            });
            diskInfoProvider.Setup(p => p.HasFreeSpace(It.IsAny<string>(), It.IsAny<long>())).Returns(true);
            modWriter.Setup(p => p.ModDirectoryExists(It.IsAny<ModWriterParameters>())).Returns((ModWriterParameters p) =>
            {
                return false;
            });

            var service = new ModMergeService(null, diskInfoProvider.Object, null, new Cache(), messageBus.Object, modPatchExporter.Object, modMergeExporter.Object,
                new List<IDefinitionInfoProvider> { infoProvider.Object }, reader.Object, modWriter.Object,
                modParser.Object, gameService.Object, storageProvider.Object, mapper.Object);

            var result = await service.HasEnoughFreeSpaceAsync("test");

            result.Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method Should_not_have_free_space_to_create_merge_mod.
        /// </summary>
        [Fact]
        public async Task Should_not_have_free_space_to_create_merge_mod()
        {
            DISetup.SetupContainer();

            var messageBus = new Mock<IMessageBus>();
            messageBus.Setup(p => p.PublishAsync(It.IsAny<IMessageBusEvent>()));
            messageBus.Setup(p => p.Publish(It.IsAny<IMessageBusEvent>()));
            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            var modMergeExporter = new Mock<IModMergeExporter>();
            var infoProvider = new Mock<IDefinitionInfoProvider>();
            var diskInfoProvider = new Mock<IDriveInfoProvider>();

            modMergeExporter.Setup(p => p.ExportFilesAsync(It.IsAny<ModMergeFileExporterParameters>())).Returns(Task.FromResult(true));
            gameService.Setup(p => p.GetSelected()).Returns(new Game
            {
                Type = "Should_not_have_free_space_to_create_merge_mod",
                UserDirectory = "C:\\Users\\Fake",
                WorkshopDirectory = new List<string> { "C:\\fake" },
                CustomModDirectory = string.Empty
            });
            var collections = new List<IModCollection> { new ModCollection { IsSelected = true, Mods = new List<string> { "mod/fakemod.mod" }, Name = "test", Game = "Should_not_have_free_space_to_create_merge_mod" } };
            storageProvider.Setup(s => s.GetModCollections()).Returns(() =>
            {
                return collections;
            });
            var fileInfos = new List<IFileInfo> { new FileInfo { Content = new List<string> { "a" }, FileName = "fakemod.mod", IsBinary = false } };
            reader.Setup(s => s.Read(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<bool>())).Returns(fileInfos);
            modParser.Setup(s => s.Parse(It.IsAny<IEnumerable<string>>(), It.IsAny<DescriptorModType>())).Returns((IEnumerable<string> values, DescriptorModType t) =>
            {
                return new ModObject { FileName = values.First(), Name = values.First() };
            });
            mapper.Setup(s => s.Map<IMod>(It.IsAny<IModObject>())).Returns((IModObject o) =>
            {
                return new Mod { FileName = o.FileName, Name = o.Name };
            });
            diskInfoProvider.Setup(p => p.HasFreeSpace(It.IsAny<string>(), It.IsAny<long>())).Returns(false);
            modWriter.Setup(p => p.ModDirectoryExists(It.IsAny<ModWriterParameters>())).Returns((ModWriterParameters p) =>
            {
                return false;
            });

            var service = new ModMergeService(null, diskInfoProvider.Object, null, new Cache(), messageBus.Object, modPatchExporter.Object, modMergeExporter.Object,
                new List<IDefinitionInfoProvider> { infoProvider.Object }, reader.Object, modWriter.Object,
                modParser.Object, gameService.Object, storageProvider.Object, mapper.Object);

            var result = await service.HasEnoughFreeSpaceAsync("test");

            result.Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method Should_not_allow_merge_when_no_game_set.
        /// </summary>
        [Fact]
        public async Task Should_not_allow_merge_when_no_game_set()
        {
            var messageBus = new Mock<IMessageBus>();
            messageBus.Setup(p => p.PublishAsync(It.IsAny<IMessageBusEvent>()));
            messageBus.Setup(p => p.Publish(It.IsAny<IMessageBusEvent>()));
            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            var modMergeExporter = new Mock<IModMergeExporter>();
            var infoProvider = new Mock<IDefinitionInfoProvider>();
            gameService.Setup(p => p.GetSelected()).Returns((IGame)null);

            var service = new ModMergeService(null, null, null, new Cache(), messageBus.Object, modPatchExporter.Object, modMergeExporter.Object,
                new List<IDefinitionInfoProvider> { infoProvider.Object }, reader.Object, modWriter.Object,
                modParser.Object, gameService.Object, storageProvider.Object, mapper.Object);

            var result = await service.AllowModMergeAsync("test");

            result.Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method Should_not_allow_merge_when_no_collection_name.
        /// </summary>
        [Fact]
        public async Task Should_not_allow_merge_when_no_collection_name()
        {
            var messageBus = new Mock<IMessageBus>();
            messageBus.Setup(p => p.PublishAsync(It.IsAny<IMessageBusEvent>()));
            messageBus.Setup(p => p.Publish(It.IsAny<IMessageBusEvent>()));
            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            var modMergeExporter = new Mock<IModMergeExporter>();
            var infoProvider = new Mock<IDefinitionInfoProvider>();
            gameService.Setup(p => p.GetSelected()).Returns(new Game
            {
                Type = "Should_not_allow_merge_when_no_collection_name",
                UserDirectory = "C:\\Users\\Fake",
                WorkshopDirectory = new List<string> { "C:\\fake" },
                CustomModDirectory = string.Empty
            });

            var service = new ModMergeService(null, null, null, new Cache(), messageBus.Object, modPatchExporter.Object, modMergeExporter.Object,
                new List<IDefinitionInfoProvider> { infoProvider.Object }, reader.Object, modWriter.Object,
                modParser.Object, gameService.Object, storageProvider.Object, mapper.Object);

            var result = await service.AllowModMergeAsync(string.Empty);

            result.Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method Should_not_allow_merge_when_no_collection_mods.
        /// </summary>
        [Fact]
        public async Task Should_not_allow_merge_when_no_collection_mods()
        {
            var messageBus = new Mock<IMessageBus>();
            messageBus.Setup(p => p.PublishAsync(It.IsAny<IMessageBusEvent>()));
            messageBus.Setup(p => p.Publish(It.IsAny<IMessageBusEvent>()));
            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            var modMergeExporter = new Mock<IModMergeExporter>();
            var infoProvider = new Mock<IDefinitionInfoProvider>();
            gameService.Setup(p => p.GetSelected()).Returns(new Game
            {
                Type = "Should_not_allow_merge_when_no_collection_mods",
                UserDirectory = "C:\\Users\\Fake",
                WorkshopDirectory = new List<string> { "C:\\fake" },
                CustomModDirectory = string.Empty
            });

            var service = new ModMergeService(null, null, null, new Cache(), messageBus.Object, modPatchExporter.Object, modMergeExporter.Object,
                new List<IDefinitionInfoProvider> { infoProvider.Object }, reader.Object, modWriter.Object,
                modParser.Object, gameService.Object, storageProvider.Object, mapper.Object);

            var result = await service.AllowModMergeAsync("test");

            result.Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method Should_not_allow_merge_mods.
        /// </summary>
        [Fact]
        public async Task Should_not_allow_merge_mods()
        {
            DISetup.SetupContainer();

            var messageBus = new Mock<IMessageBus>();
            messageBus.Setup(p => p.PublishAsync(It.IsAny<IMessageBusEvent>()));
            messageBus.Setup(p => p.Publish(It.IsAny<IMessageBusEvent>()));
            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            var modMergeExporter = new Mock<IModMergeExporter>();
            var infoProvider = new Mock<IDefinitionInfoProvider>();
            var diskInfoProvider = new Mock<IDriveInfoProvider>();

            modMergeExporter.Setup(p => p.ExportFilesAsync(It.IsAny<ModMergeFileExporterParameters>())).Returns(Task.FromResult(true));
            gameService.Setup(p => p.GetSelected()).Returns(new Game { Type = "Should_not_allow_merge_mods", UserDirectory = "C:\\Users\\Fake", WorkshopDirectory = new List<string> { "C:\\fake" }, CustomModDirectory = string.Empty });
            var collections = new List<IModCollection> { new ModCollection { IsSelected = true, Mods = new List<string> { "mod/fakemod.mod" }, Name = "test", Game = "Should_not_allow_merge_mods" } };
            storageProvider.Setup(s => s.GetModCollections()).Returns(() =>
            {
                return collections;
            });
            var fileInfos = new List<IFileInfo> { new FileInfo { Content = new List<string> { "a" }, FileName = "fakemod.mod", IsBinary = false } };
            reader.Setup(s => s.Read(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<bool>())).Returns(fileInfos);
            modParser.Setup(s => s.Parse(It.IsAny<IEnumerable<string>>(), It.IsAny<DescriptorModType>())).Returns((IEnumerable<string> values, DescriptorModType t) =>
            {
                return new ModObject { FileName = values.First(), Name = values.First() };
            });
            mapper.Setup(s => s.Map<IMod>(It.IsAny<IModObject>())).Returns((IModObject o) =>
            {
                return new Mod { FileName = o.FileName, Name = o.Name, FullPath = "C:\\Users\\Fake\\mod\\test" };
            });
            diskInfoProvider.Setup(p => p.HasFreeSpace(It.IsAny<string>(), It.IsAny<long>())).Returns(true);
            modWriter.Setup(p => p.ModDirectoryExists(It.IsAny<ModWriterParameters>())).Returns((ModWriterParameters p) =>
            {
                return false;
            });

            var service = new ModMergeService(null, diskInfoProvider.Object, null, new Cache(), messageBus.Object, modPatchExporter.Object, modMergeExporter.Object,
                new List<IDefinitionInfoProvider> { infoProvider.Object }, reader.Object, modWriter.Object,
                modParser.Object, gameService.Object, storageProvider.Object, mapper.Object);

            var result = await service.AllowModMergeAsync("test");

            result.Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method Should_allow_merge_mods.
        /// </summary>
        [Fact]
        public async Task Should_allow_merge_mods()
        {
            DISetup.SetupContainer();

            var messageBus = new Mock<IMessageBus>();
            messageBus.Setup(p => p.PublishAsync(It.IsAny<IMessageBusEvent>()));
            messageBus.Setup(p => p.Publish(It.IsAny<IMessageBusEvent>()));
            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            var modMergeExporter = new Mock<IModMergeExporter>();
            var infoProvider = new Mock<IDefinitionInfoProvider>();
            var diskInfoProvider = new Mock<IDriveInfoProvider>();

            modMergeExporter.Setup(p => p.ExportFilesAsync(It.IsAny<ModMergeFileExporterParameters>())).Returns(Task.FromResult(true));
            gameService.Setup(p => p.GetSelected()).Returns(new Game { Type = "Should_allow_merge_mods", UserDirectory = "C:\\Users\\Fake", WorkshopDirectory = new List<string> { "C:\\fake" }, CustomModDirectory = string.Empty });
            var collections = new List<IModCollection> { new ModCollection { IsSelected = true, Mods = new List<string> { "mod/fakemod.mod" }, Name = "test", Game = "Should_allow_merge_mods" } };
            storageProvider.Setup(s => s.GetModCollections()).Returns(() =>
            {
                return collections;
            });
            var fileInfos = new List<IFileInfo> { new FileInfo { Content = new List<string> { "a" }, FileName = "fakemod.mod", IsBinary = false } };
            reader.Setup(s => s.Read(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<bool>())).Returns(fileInfos);
            modParser.Setup(s => s.Parse(It.IsAny<IEnumerable<string>>(), It.IsAny<DescriptorModType>())).Returns((IEnumerable<string> values, DescriptorModType t) =>
            {
                return new ModObject { FileName = values.First(), Name = values.First() };
            });
            mapper.Setup(s => s.Map<IMod>(It.IsAny<IModObject>())).Returns((IModObject o) =>
            {
                return new Mod { FileName = o.FileName, Name = o.Name, FullPath = "C:\\Users\\Fake\\mod\\faketest" };
            });
            diskInfoProvider.Setup(p => p.HasFreeSpace(It.IsAny<string>(), It.IsAny<long>())).Returns(true);
            modWriter.Setup(p => p.ModDirectoryExists(It.IsAny<ModWriterParameters>())).Returns((ModWriterParameters p) =>
            {
                return false;
            });

            var service = new ModMergeService(null, diskInfoProvider.Object, null, new Cache(), messageBus.Object, modPatchExporter.Object, modMergeExporter.Object,
                new List<IDefinitionInfoProvider> { infoProvider.Object }, reader.Object, modWriter.Object,
                modParser.Object, gameService.Object, storageProvider.Object, mapper.Object);

            var result = await service.AllowModMergeAsync("test");

            result.Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method Should_return_mod_merge_collection.
        /// </summary>
        [Fact]
        public void Should_return_merge_collection()
        {
            var preferencesService = new Mock<IPreferencesService>();
            preferencesService.Setup(p => p.Get()).Returns(() =>
                new Preferences { MergeCollectionModNameTemplate = "{Name} test {Merged}" });

            var service = new ModMergeService(preferencesService.Object, null, null, null, null, null, null, null, null, null, null, null, null, null);
            var result = service.GetMergeCollectionNameTemplate();
            result.Should().Be("{Name} test {Merged}");
        }

        /// <summary>
        /// Defines the test method Should_not_return_merge_collection.
        /// </summary>
        [Fact]
        public void Should_not_return_merge_collection()
        {
            var preferencesService = new Mock<IPreferencesService>();
            preferencesService.Setup(p => p.Get()).Returns(() =>
                new Preferences { MergedCollectionNameTemplate = "{lame} test" });

            var service = new ModMergeService(preferencesService.Object, null, null, null, null, null, null, null, null, null, null, null, null, null);
            var result = service.GetMergeCollectionNameTemplate();
            result.Should().BeNullOrWhiteSpace();
        }

        /// <summary>
        /// Defines the test method Should_return_mod_merge_collection.
        /// </summary>
        [Fact]
        public void Should_return_mod_merge_collection()
        {
            var preferencesService = new Mock<IPreferencesService>();
            preferencesService.Setup(p => p.Get()).Returns(() =>
                new Preferences { MergedCollectionNameTemplate = "{Name} test {Merged}" });

            var service = new ModMergeService(preferencesService.Object, null, null, null, null, null, null, null, null, null, null, null, null, null);
            var result = service.GetMergeCollectionModNameTemplate();
            result.Should().Be("{Name} test {Merged}");
        }

        /// <summary>
        /// Defines the test method Should_not_return_mod_merge_collection.
        /// </summary>
        [Fact]
        public void Should_not_return_mod_merge_collection()
        {
            var preferencesService = new Mock<IPreferencesService>();
            preferencesService.Setup(p => p.Get()).Returns(() =>
                new Preferences { MergedCollectionNameTemplate = "{lame} test" });

            var service = new ModMergeService(preferencesService.Object, null, null, null, null, null, null, null, null, null, null, null, null, null);
            var result = service.GetMergeCollectionModNameTemplate();
            result.Should().BeNullOrWhiteSpace();
        }


        /// <summary>
        /// Defines the test method Should_save_mod_merge_collection.
        /// </summary>
        [Fact]
        public void Should_save_merge_collection()
        {
            var preferencesService = new Mock<IPreferencesService>();
            preferencesService.Setup(p => p.Get()).Returns(() =>
                new Preferences { MergedCollectionNameTemplate = "{Name} test {Merged}" });
            preferencesService.Setup(p => p.Save(It.IsAny<IPreferences>())).Returns((IPreferences saved) => true);

            var service = new ModMergeService(preferencesService.Object, null, null, null, null, null, null, null, null, null, null, null, null, null);
            var result = service.SaveMergedCollectionNameTemplate("{Name} test {Merged}");
            result.Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method Should_not_save_merge_collection.
        /// </summary>
        [Fact]
        public void Should_not_save_merge_collection()
        {
            var preferencesService = new Mock<IPreferencesService>();
            preferencesService.Setup(p => p.Get()).Returns(() =>
                new Preferences { MergedCollectionNameTemplate = "{lame} test" });
            preferencesService.Setup(p => p.Save(It.IsAny<IPreferences>())).Returns((IPreferences saved) => false);

            var service = new ModMergeService(preferencesService.Object, null, null, null, null, null, null, null, null, null, null, null, null, null);
            var result = service.SaveMergedCollectionNameTemplate("{lame} test");
            result.Should().BeFalse();
        }


        /// <summary>
        /// Defines the test method Should_save_mod_merge_collection.
        /// </summary>
        [Fact]
        public void Should_save_mod_merge_collection()
        {
            var preferencesService = new Mock<IPreferencesService>();
            preferencesService.Setup(p => p.Get()).Returns(() =>
                new Preferences { MergedCollectionNameTemplate = "{Name} test" });
            preferencesService.Setup(p => p.Save(It.IsAny<IPreferences>())).Returns((IPreferences saved) => true);

            var service = new ModMergeService(preferencesService.Object, null, null, null, null, null, null, null, null, null, null, null, null, null);
            var result = service.SaveMergeCollectionModNameTeplate("{Name} test {Merged}");
            result.Should().BeTrue();
        }


        /// <summary>
        /// Defines the test method Should_not_save_mod_merge_collection.
        /// </summary>
        [Fact]
        public void Should_not_save_mod_merge_collection()
        {
            var preferencesService = new Mock<IPreferencesService>();
            preferencesService.Setup(p => p.Get()).Returns(() =>
                new Preferences { MergedCollectionNameTemplate = "{lame} test" });
            preferencesService.Setup(p => p.Save(It.IsAny<IPreferences>())).Returns((IPreferences saved) => false);

            var service = new ModMergeService(preferencesService.Object, null, null, null, null, null, null, null, null, null, null, null, null, null);
            var result = service.SaveMergeCollectionModNameTeplate("{lame} test");
            result.Should().BeFalse();
        }

        /// <summary>
        /// Class DomainConfigDummy.
        /// Implements the <see cref="IDomainConfiguration" />
        /// </summary>
        /// <seealso cref="IDomainConfiguration" />
        private class DomainConfigDummy : IDomainConfiguration
        {
            /// <summary>
            /// The domain
            /// </summary>
            private readonly DomainConfigurationOptions domain = new();

            /// <summary>
            /// Initializes a new instance of the <see cref="DomainConfigDummy" /> class.
            /// </summary>
            /// <param name="useLegacySteamLaunch">if set to <c>true</c> [use legacy steam launch].</param>
            public DomainConfigDummy(bool useLegacySteamLaunch)
            {
                domain.Steam.UseLegacyLaunchMethod = useLegacySteamLaunch;
            }

            /// <summary>
            /// Gets the options.
            /// </summary>
            /// <returns>DomainConfigurationOptions.</returns>
            public DomainConfigurationOptions GetOptions()
            {
                return domain;
            }
        }
    }
}
