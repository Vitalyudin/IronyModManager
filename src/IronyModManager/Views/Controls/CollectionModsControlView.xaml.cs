﻿// ***********************************************************************
// Assembly         : IronyModManager
// Author           : Mario
// Created          : 03-03-2020
//
// Last Modified By : Mario
// Last Modified On : 04-27-2020
// ***********************************************************************
// <copyright file="CollectionModsControlView.xaml.cs" company="Mario">
//     Mario
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using IronyModManager.Common.Views;
using IronyModManager.Controls;
using IronyModManager.Models.Common;
using IronyModManager.Shared;
using IronyModManager.ViewModels.Controls;
using ReactiveUI;

namespace IronyModManager.Views.Controls
{
    /// <summary>
    /// Class CollectionModsControlView.
    /// Implements the <see cref="IronyModManager.Common.Views.BaseControl{IronyModManager.ViewModels.Controls.CollectionModsControlViewModel}" />
    /// </summary>
    /// <seealso cref="IronyModManager.Common.Views.BaseControl{IronyModManager.ViewModels.Controls.CollectionModsControlViewModel}" />
    [ExcludeFromCoverage("This should be tested via functional testing.")]
    public class CollectionModsControlView : BaseControl<CollectionModsControlViewModel>
    {
        #region Fields

        /// <summary>
        /// The order name
        /// </summary>
        private const string OrderName = "order";

        /// <summary>
        /// The mod list
        /// </summary>
        private DragDropListBox modList;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionModsControlView" /> class.
        /// </summary>
        public CollectionModsControlView()
        {
            this.InitializeComponent();
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// focus order textbox as an asynchronous operation.
        /// </summary>
        /// <param name="mod">The mod.</param>
        protected virtual async Task FocusOrderTextboxAsync(IMod mod)
        {
            await Task.Delay(100);
            var listboxItems = modList.GetLogicalChildren().Cast<ListBoxItem>();
            if (mod != null)
            {
                foreach (var item in listboxItems)
                {
                    var grid = item.GetLogicalChildren().OfType<Grid>().FirstOrDefault();
                    if (grid != null)
                    {
                        var contentMod = item.Content as IMod;
                        if (mod == contentMod)
                        {
                            grid.Focus();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the item dragged.
        /// </summary>
        protected virtual void HandleItemDragged()
        {
            modList.ItemDragged += (source, destination) =>
            {
                var sourceMod = source as IMod;
                var destinationMod = destination as IMod;
                sourceMod.Order = destinationMod.Order;
            };
        }

        /// <summary>
        /// Called when [activated].
        /// </summary>
        /// <param name="disposables">The disposables.</param>
        protected override void OnActivated(CompositeDisposable disposables)
        {
            modList = this.FindControl<DragDropListBox>("modList");
            if (modList != null)
            {
                SetContextMenus();
                SetOrderParameters();
                HandleItemDragged();
            }
            base.OnActivated(disposables);
        }

        /// <summary>
        /// Sets the pointer events.
        /// </summary>
        protected virtual void SetContextMenus()
        {
            modList.PointerMoved += (sender, args) =>
            {
                var hoveredItem = modList.GetLogicalChildren().Cast<ListBoxItem>().FirstOrDefault(p => p.IsPointerOver);
                if (hoveredItem != null)
                {
                    var grid = hoveredItem.GetLogicalChildren().OfType<Grid>().FirstOrDefault();
                    if (grid != null)
                    {
                        ViewModel.HoveredMod = hoveredItem.Content as IMod;
                        if (!string.IsNullOrEmpty(ViewModel.GetHoveredModUrl()) || !string.IsNullOrEmpty(ViewModel.GetHoveredModSteamUrl()))
                        {
                            var menuItems = new List<MenuItem>();
                            if (!string.IsNullOrEmpty(ViewModel.GetHoveredModUrl()))
                            {
                                menuItems.Add(new MenuItem()
                                {
                                    Header = ViewModel.OpenUrl,
                                    Command = ViewModel.OpenUrlCommand
                                });
                                menuItems.Add(new MenuItem()
                                {
                                    Header = ViewModel.CopyUrl,
                                    Command = ViewModel.CopyUrlCommand
                                });
                            }
                            if (!string.IsNullOrEmpty(ViewModel.GetHoveredModSteamUrl()))
                            {
                                var menuItem = new MenuItem()
                                {
                                    Header = ViewModel.OpenInSteam,
                                    Command = ViewModel.OpenInSteamCommand
                                };
                                if (menuItems.Count == 0)
                                {
                                    menuItems.Add(menuItem);
                                }
                                else
                                {
                                    menuItems.Insert(1, menuItem);
                                }
                            }
                            if (!string.IsNullOrWhiteSpace(ViewModel.HoveredMod?.FullPath))
                            {
                                var menuItem = new MenuItem()
                                {
                                    Header = ViewModel.OpenInAssociatedApp,
                                    Command = ViewModel.OpenInAssociatedAppCommand
                                };
                                if (menuItems.Count == 0)
                                {
                                    menuItems.Add(menuItem);
                                }
                                else
                                {
                                    menuItems.Insert(0, menuItem);
                                }
                            }
                            grid.ContextMenu.Items = menuItems;
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Sets the order parameters.
        /// </summary>
        protected virtual void SetOrderParameters()
        {
            void setMaxValue()
            {
                var listboxItems = modList.GetLogicalChildren().Cast<ListBoxItem>();
                foreach (var item in listboxItems)
                {
                    var grid = item.GetLogicalChildren().OfType<Grid>().FirstOrDefault();
                    if (grid != null)
                    {
                        var orderCtrl = grid.GetLogicalChildren().OfType<MinMaxNumericUpDown>().FirstOrDefault(p => p.Name == OrderName);
                        if (orderCtrl != null)
                        {
                            orderCtrl.Minimum = 1;
                            orderCtrl.Maximum = ViewModel.MaxOrder;
                        }
                    }
                }
            }

            ViewModel.ModReordered += (args) =>
            {
                setMaxValue();
                FocusOrderTextboxAsync(args).ConfigureAwait(true);
            };

            this.WhenAnyValue(v => v.ViewModel.MaxOrder).Subscribe(max =>
            {
                setMaxValue();
            }).DisposeWith(Disposables);

            modList.LayoutUpdated += (sender, args) =>
            {
                setMaxValue();
            };
        }

        /// <summary>
        /// Initializes the component.
        /// </summary>
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        #endregion Methods
    }
}
