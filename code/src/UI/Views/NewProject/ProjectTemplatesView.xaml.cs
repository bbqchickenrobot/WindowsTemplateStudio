﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Windows.Controls;

using Microsoft.Templates.UI.ViewModels.NewProject;

namespace Microsoft.Templates.UI.Views.NewProject
{
    /// <summary>
    /// Interaction logic for ProjectTemplatesView.xaml
    /// </summary>
    public partial class ProjectTemplatesView : Page
    {
        public ProjectTemplatesViewModel ViewModel { get; }

        public ProjectTemplatesView()
        {
            ViewModel = MainViewModel.Current.ProjectTemplates;
            DataContext = ViewModel;

            Loaded += async (sender, e) =>
            {
                await ViewModel.InitializeAsync();
            };

            InitializeComponent();
        }
    }
}
