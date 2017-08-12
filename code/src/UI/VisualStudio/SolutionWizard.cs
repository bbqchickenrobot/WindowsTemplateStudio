﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.IO;
using EnvDTE;
using Microsoft.Templates.Core;
using Microsoft.Templates.Core.Diagnostics;
using Microsoft.Templates.Core.Gen;
using Microsoft.Templates.Core.Locations;
using Microsoft.Templates.Core.PostActions.Catalog.Merge;
using Microsoft.Templates.UI.Resources;
using Microsoft.VisualStudio.TemplateWizard;

namespace Microsoft.Templates.UI.VisualStudio
{
    public abstract class SolutionWizard : IWizard, IContextProvider
    {
        private UserSelection _userSelection;
        private Dictionary<string, string> _replacementsDictionary;
        private string _language;

        public string ProjectName => _replacementsDictionary["$safeprojectname$"];

        public string ProjectPath => new DirectoryInfo(_replacementsDictionary["$destinationdirectory$"]).FullName;

        public string OutputPath => ProjectPath;

        public List<string> ProjectItems { get; } = new List<string>();

        public List<FailedMergePostAction> FailedMergePostActions { get; } = new List<FailedMergePostAction>();

        public Dictionary<string, List<MergeInfo>> MergeFilesFromProject { get; } = new Dictionary<string, List<MergeInfo>>();
        public List<string> FilesToOpen { get; } = new List<string>();

        protected void Initialize(string language)
        {
            _language = language;

            if (GenContext.InitializedLanguage != language)
            {
#if DEBUG
                GenContext.Bootstrap(new LocalTemplatesSource(), new VsGenShell(), language);
#else
                GenContext.Bootstrap(new RemoteTemplatesSource(), new VsGenShell(), language);
#endif
                }
        }

        public void BeforeOpeningFile(ProjectItem projectItem)
        {
        }

        public void ProjectFinishedGenerating(Project project)
        {
        }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
        }

        public async void RunFinished()
        {
            AppHealth.Current.Info.TrackAsync(StringRes.SolutionWizardRunFinishedMessage).FireAndForget();
            await NewProjectGenController.Instance.GenerateProjectAsync(_userSelection);
            AppHealth.Current.Info.TrackAsync(StringRes.GenerationFinishedString).FireAndForget();

            PostGenerationActions();
        }

        private static void PostGenerationActions()
        {
            GenContext.ToolBox.Shell.ShowStatusBarMessage(StringRes.RestoringMessage);
            GenContext.ToolBox.Shell.RestorePackages();

            GenContext.ToolBox.Shell.CollapseSolutionItems();
        }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            var solutionDirectory = replacementsDictionary["$solutiondirectory$"];

            try
            {
                if (runKind == WizardRunKind.AsNewProject || runKind == WizardRunKind.AsMultiProject)
                {
                    _replacementsDictionary = replacementsDictionary;

                    GenContext.Current = this;

                    _userSelection = NewProjectGenController.Instance.GetUserSelection(_language);
                }
            }
            catch (WizardBackoutException)
            {
                if (Directory.Exists(solutionDirectory))
                {
                    Directory.Delete(solutionDirectory, true);
                }

                throw;
            }
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }
    }
}
