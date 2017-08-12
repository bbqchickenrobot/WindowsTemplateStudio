﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Templates.Core.Diagnostics;
using Microsoft.Templates.Core.Resources;
using Microsoft.Templates.Core.Locations;

namespace Microsoft.Templates.Core.Gen
{
    public class GenContext
    {
        private static IContextProvider _currentContext;
        private static string _tempGenerationFolder = Path.Combine(Path.GetTempPath(), Configuration.Current.TempGenerationFolderPath);

        public static GenToolBox ToolBox { get; private set; }
        public static string InitializedLanguage { get; private set; }

        public static IContextProvider Current
        {
            get
            {
                if (_currentContext == null)
                {
                    throw new InvalidOperationException(StringRes.GenContextCurrentInvalidOperationMessage);
                }

                return _currentContext;
            }

            set
            {
                _currentContext = value;
            }
        }

        public static void Bootstrap(TemplatesSource source, GenShell shell, string language)
        {
            Bootstrap(source, shell, GetWizardVersionFromAssembly(), language);
        }

        public static void Bootstrap(TemplatesSource source, GenShell shell, Version wizardVersion, string language)
        {
            try
            {
                AppHealth.Current.AddWriter(new ShellHealthWriter(shell));
                AppHealth.Current.Info.TrackAsync($"{StringRes.ConfigurationFileLoadedString}: {Configuration.LoadedConfigFile}").FireAndForget();

                string hostVersion = $"{wizardVersion.Major}.{wizardVersion.Minor}";

                CodeGen.Initialize(source.Id, hostVersion);
                var repository = new TemplatesRepository(source, wizardVersion, language);

                ToolBox = new GenToolBox(repository, shell);

                PurgeTempGenerations(Configuration.Current.DaysToKeepTempGenerations);

                CodeGen.Initialize(source.Id, hostVersion);

                InitializedLanguage = language;
            }
            catch (Exception ex)
            {
                AppHealth.Current.Exception.TrackAsync(ex, StringRes.GenContextBootstrapError).FireAndForget();
                Trace.TraceError($"{StringRes.GenContextBootstrapError} Exception:\n\r{ex}");
                throw;
            }
        }

        public static string GetTempGenerationPath(string projectName)
        {
            Fs.EnsureFolder(_tempGenerationFolder);

            var tempGenerationName = $"{projectName}_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}";
            var inferredName = Naming.Infer(tempGenerationName, new List<Validator>() { new DirectoryExistsValidator(_tempGenerationFolder) }, "_");

            return Path.Combine(_tempGenerationFolder, inferredName);
        }

        private static void PurgeTempGenerations(int daysToKeep)
        {
            if (Directory.Exists(_tempGenerationFolder))
            {
                var di = new DirectoryInfo(_tempGenerationFolder);
                var toBeDeleted = di.GetDirectories().Where(d => d.CreationTimeUtc.AddDays(daysToKeep) < DateTime.UtcNow);

                foreach (var d in toBeDeleted)
                {
                    try
                    {
                        d.Delete(true);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error removing old temp generation directory '{d.FullName}'. Skipped. Exception:\n\r{ex.ToString()}");
                        Trace.TraceError($"Error removing old temp generation directory '{d.FullName}'. Skipped. Exception:\n\r{ex.ToString()}");
                    }
                }
            }
        }

        private static Version GetWizardVersionFromAssembly()
        {
            string assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var versionInfo = FileVersionInfo.GetVersionInfo(assemblyLocation);

            Version.TryParse(versionInfo.FileVersion, out Version v);

            return v;
        }
    }
}
