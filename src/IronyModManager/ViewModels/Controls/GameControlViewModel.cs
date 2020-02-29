﻿// ***********************************************************************
// Assembly         : IronyModManager
// Author           : Mario
// Created          : 02-12-2020
//
// Last Modified By : Mario
// Last Modified On : 02-29-2020
// ***********************************************************************
// <copyright file="GameControlViewModel.cs" company="Mario">
//     Mario
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using IronyModManager.Common.Events;
using IronyModManager.Common.ViewModels;
using IronyModManager.Localization.Attributes;
using IronyModManager.Models.Common;
using IronyModManager.Services.Common;
using IronyModManager.Shared;
using ReactiveUI;

namespace IronyModManager.ViewModels.Controls
{
    /// <summary>
    /// Class GameControlViewModel.
    /// Implements the <see cref="IronyModManager.Common.ViewModels.BaseViewModel" />
    /// </summary>
    /// <seealso cref="IronyModManager.Common.ViewModels.BaseViewModel" />
    [ExcludeFromCoverage("This should be tested via functional testing.")]
    public class GameControlViewModel : BaseViewModel
    {
        #region Fields

        /// <summary>
        /// The game service
        /// </summary>
        private readonly IGameService gameService;

        /// <summary>
        /// The previous game
        /// </summary>
        private IGame previousGame;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GameControlViewModel" /> class.
        /// </summary>
        /// <param name="gameService">The game service.</param>
        public GameControlViewModel(IGameService gameService)
        {
            this.gameService = gameService;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the games.
        /// </summary>
        /// <value>The games.</value>
        [AutoRefreshLocalization]
        public virtual IEnumerable<IGame> Games { get; protected set; }

        /// <summary>
        /// Gets or sets the game text.
        /// </summary>
        /// <value>The game text.</value>
        [StaticLocalization(LocalizationResources.Games.Name)]
        public virtual string GameText { get; protected set; }

        /// <summary>
        /// Gets or sets the selected game.
        /// </summary>
        /// <value>The selected game.</value>
        [AutoRefreshLocalization]
        public virtual IGame SelectedGame { get; protected set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Called when [activated].
        /// </summary>
        /// <param name="disposables">The disposables.</param>
        protected override void OnActivated(CompositeDisposable disposables)
        {
            Games = gameService.Get();

            previousGame = SelectedGame = Games.FirstOrDefault(s => s.IsSelected);

            var changed = this.WhenAnyValue(s => s.SelectedGame).Subscribe(s =>
            {
                if (Games?.Count() > 0 && s != null)
                {
                    if (gameService.SetSelected(Games, s) && previousGame != s)
                    {
                        var args = new SelectedGameChangedEventArgs()
                        {
                            Game = s
                        };
                        MessageBus.Current.SendMessage(args);
                        previousGame = s;
                    }
                }
            }).DisposeWith(disposables);

            base.OnActivated(disposables);
        }

        #endregion Methods
    }
}
