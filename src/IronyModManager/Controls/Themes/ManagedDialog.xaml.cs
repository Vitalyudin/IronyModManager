﻿// ***********************************************************************
// Assembly         : IronyModManager
// Author           : Avalonia
// Created          : 05-07-2020
//
// Last Modified By : Mario
// Last Modified On : 03-22-2024
// ***********************************************************************
// <copyright file="ManagedDialog.xaml.cs" company="Avalonia">
//     Avalonia
// </copyright>
// <summary>
// Based on Avalonia ManagedFileChooser. Why of why would
// the Avalonia guys expose some of this stuff?
// </summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Dialogs;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using IronyModManager.Controls.Dialogs;
using IronyModManager.DI;
using IronyModManager.Localization;
using IronyModManager.Shared;

namespace IronyModManager.Controls.Themes
{
    /// <summary>
    /// Class ManagedDialog.
    /// Implements the <see cref="Avalonia.Controls.UserControl" />
    /// </summary>
    /// <seealso cref="Avalonia.Controls.UserControl" />
    [ExcludeFromCoverage("External logic.")]
    public class ManagedDialog : UserControl
    {
        #region Fields

        /// <summary>
        /// The file name
        /// </summary>
        private readonly TextBox fileName;

        /// <summary>
        /// The files view
        /// </summary>
        private readonly Avalonia.Controls.ListBox filesView;

        /// <summary>
        /// The filter
        /// </summary>
        private readonly ComboBox filter;

        /// <summary>
        /// The quick links root
        /// </summary>
        private readonly Control quickLinksRoot;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedDialog" /> class.
        /// </summary>
        public ManagedDialog()
        {
            AvaloniaXamlLoader.Load(this);
            AddHandler(PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
            quickLinksRoot = this.FindControl<Control>("QuickLinks");
            filesView = this.FindControl<Avalonia.Controls.ListBox>("Files");
            var locManager = DIResolver.Get<ILocalizationManager>();
            fileName = this.FindControl<TextBox>("fileName");
            fileName.Watermark = locManager.GetResource(LocalizationResources.FileDialog.FileName);
            filter = this.FindControl<ComboBox>("filter");
            var correctingInput = false;
            fileName.PropertyChanged += (_, args) =>
            {
                if (args.Property != Avalonia.Controls.TextBox.TextProperty)
                {
                    return;
                }

                if (correctingInput)
                {
                    return;
                }

                correctingInput = true;

                async Task updateText()
                {
                    await Task.Delay(1);
                    Model.FileName = Model.FileName.GenerateValidFileName(false);
                    correctingInput = false;
                }

                updateText().ConfigureAwait(false);
            };
            var showHiddenFiles = this.FindControl<TextBlock>("showHiddenFiles");
            showHiddenFiles.Text = locManager.GetResource(LocalizationResources.FileDialog.ShowHiddenFiles);
            var ok = this.FindControl<Button>("ok");
            ok.Content = locManager.GetResource(LocalizationResources.FileDialog.OK);
            var cancel = this.FindControl<Button>("cancel");
            var name = this.FindControl<TextBlock>("name");
            name.Text = locManager.GetResource(LocalizationResources.FileDialog.Name);
            var date = this.FindControl<TextBlock>("dateModified");
            date.Text = locManager.GetResource(LocalizationResources.FileDialog.DateModified);
            var type = this.FindControl<TextBlock>("type");
            type.Text = locManager.GetResource(LocalizationResources.FileDialog.Type);
            var size = this.FindControl<TextBlock>("size");
            size.Text = locManager.GetResource(LocalizationResources.FileDialog.Size);
            cancel.Content = locManager.GetResource(LocalizationResources.FileDialog.Cancel);
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <value>The model.</value>
        private ManagedDialogViewModel Model => DataContext as ManagedDialogViewModel;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Handles the <see cref="E:DataContextChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected override async void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);

            var model = DataContext as ManagedDialogViewModel;

            if (model == null)
            {
                return;
            }

            var preselected = model.SelectedItems.FirstOrDefault();

            if (preselected == null)
            {
                return;
            }

            //Let everything to settle down and scroll to selected item
            await Task.Delay(100);

            if (preselected != model.SelectedItems.FirstOrDefault())
            {
                return;
            }

            // Workaround for ListBox bug, scroll to the previous file
            var indexOfPreselected = model.Items.IndexOf(preselected);

            if (indexOfPreselected > 1)
            {
                filesView.ScrollIntoView(indexOfPreselected - 1);
            }
        }

        /// <summary>
        /// Handles the <see cref="E:PointerPressed" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PointerPressedEventArgs" /> instance containing the event data.</param>
        private void OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            async Task deselectItems()
            {
                if (filesView.IsLogicalAncestorOf(e.Source as Control) && !filter.IsLogicalAncestorOf(e.Source as Control) && !fileName.IsLogicalAncestorOf(e.Source as Control))
                {
                    await Task.Delay(25);
                    if (Model.SelectedItems?.Count > 0)
                    {
                        if (Model.SelectedItems.Any(p => p.Path.EndsWith(Model.FileName ?? string.Empty)))
                        {
                            Model.FileName = string.Empty;
                        }

                        Model.SelectedItems.Clear();
                    }
                }
            }

            if ((e.Source as StyledElement)?.DataContext is not ManagedDialogItemViewModel model)
            {
                Dispatcher.UIThread.InvokeAsync(() => deselectItems());
                return;
            }

            // Right click now de selects stuff
            if (e.GetCurrentPoint(null).Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed)
            {
                Dispatcher.UIThread.InvokeAsync(() => deselectItems());
                return;
            }

            var isQuickLink = quickLinksRoot.IsLogicalAncestorOf(e.Source as Control);
#pragma warning disable CS0618 // Type or member is obsolete
            // Yes, use doubletapped event... if only it would work properly.
            if (e.ClickCount == 2 || isQuickLink)
#pragma warning restore CS0618 // Type or member is obsolete
            {
                if (model.ItemType == ManagedFileChooserItemType.File)
                {
                    Model?.SelectSingleFile(model);
                }
                else
                {
                    Model?.Navigate(model.Path);
                }

                e.Handled = true;
            }
        }

        #endregion Methods
    }
}
