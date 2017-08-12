// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Templates.Core;
using Microsoft.Templates.Core.Gen;
using Microsoft.Templates.Core.Locations;
using Microsoft.Templates.Fakes;
using Microsoft.Templates.UI;

using Xunit;

namespace Microsoft.Templates.Test
{
    [Collection("Generation collection")]
    public class ProjectGenerationTests : BaseTestContextProvider
    {
        private GenerationFixture _fixture;

        public ProjectGenerationTests(GenerationFixture fixture)
        {
            _fixture = fixture;
        }

        private void SetUpFixtureForTesting(string language)
        {
            _fixture.InitializeFixture(language, this);
        }

        [Theory]
        [MemberData("GetProjectTemplates")]
        [Trait("Type", "ProjectGeneration")]
        public async void GenerateEmptyProject(string projectType, string framework, string language)
        {
            SetUpFixtureForTesting(language);

            var projectTemplate =
                GenerationFixture.Templates.FirstOrDefault(
                    t => t.GetTemplateType() == TemplateType.Project
                      && t.GetProjectTypeList().Contains(projectType)
                      && t.GetFrameworkList().Contains(framework));
            var projectName = $"{projectType}{framework}";

            ProjectName = projectName;
            ProjectPath = Path.Combine(_fixture.TestProjectsPath, projectName, projectName);
            OutputPath = ProjectPath;

            var userSelection = GenerationFixture.SetupProject(projectType, framework, language);

            await NewProjectGenController.Instance.UnsafeGenerateProjectAsync(userSelection);

            // Build solution
            var outputPath = Path.Combine(_fixture.TestProjectsPath, projectName);
            var result = GenerationFixture.BuildSolution(projectName, outputPath);

            // Assert
            Assert.True(result.exitCode.Equals(0), $"Solution {projectTemplate.Name} was not built successfully. {Environment.NewLine}Errors found: {GenerationFixture.GetErrorLines(result.outputFile)}.{Environment.NewLine}Please see {Path.GetFullPath(result.outputFile)} for more details.");

            // Clean
            Directory.Delete(outputPath, true);
        }

        [Theory]
        [MemberData("GetPageAndFeatureTemplates")]
        [Trait("Type", "OneByOneItemGeneration")]
        public async void GenerateProjectWithIsolatedItems(string itemName, string projectType, string framework, string itemId, string language)
        {
            SetUpFixtureForTesting(language);

            var projectTemplate = GenerationFixture.Templates.FirstOrDefault(t => t.GetTemplateType() == TemplateType.Project && t.GetProjectTypeList().Contains(projectType) && t.GetFrameworkList().Contains(framework));
            var itemTemplate = GenerationFixture.Templates.FirstOrDefault(t => t.Identity == itemId);
            var finalName = itemTemplate.GetDefaultName();
            var validators = new List<Validator>
            {
                new ReservedNamesValidator(),
            };
            if (itemTemplate.GetItemNameEditable())
            {
                validators.Add(new DefaultNamesValidator());
            }

            finalName = Naming.Infer(finalName, validators);

            var projectName = $"{projectType}{framework}{finalName}";

            ProjectName = projectName;
            ProjectPath = Path.Combine(_fixture.TestProjectsPath, projectName, projectName);
            OutputPath = ProjectPath;

            var userSelection = GenerationFixture.SetupProject(projectType, framework, language);

            GenerationFixture.AddItem(userSelection, itemTemplate, GenerationFixture.GetDefaultName);

            await NewProjectGenController.Instance.UnsafeGenerateProjectAsync(userSelection);

            // Build solution
            var outputPath = Path.Combine(_fixture.TestProjectsPath, projectName);
            var result = GenerationFixture.BuildSolution(projectName, outputPath);

            // Assert
            Assert.True(result.exitCode.Equals(0), $"Solution {projectTemplate.Name} was not built successfully. {Environment.NewLine}Errors found: {GenerationFixture.GetErrorLines(result.outputFile)}.{Environment.NewLine}Please see {Path.GetFullPath(result.outputFile)} for more details.");

            // Clean
            Directory.Delete(outputPath, true);
        }

        [Theory]
        [MemberData("GetProjectTemplates")]
        [Trait("Type", "ProjectGeneration")]
        public async void GenerateAllPagesAndFeatures(string projectType, string framework, string language)
        {
            SetUpFixtureForTesting(language);

            var targetProjectTemplate = GenerationFixture.Templates
                                                         .FirstOrDefault(t => t.GetTemplateType() == TemplateType.Project
                                                                           && t.GetProjectTypeList().Contains(projectType)
                                                                           && t.GetFrameworkList().Contains(framework));

            var projectName = $"{projectType}{framework}All";

            ProjectName = projectName;
            ProjectPath = Path.Combine(_fixture.TestProjectsPath, projectName, projectName);
            OutputPath = ProjectPath;

            var userSelection = GenerationFixture.SetupProject(projectType, framework, language);

            GenerationFixture.AddItems(userSelection, GenerationFixture.GetTemplates(framework), GenerationFixture.GetDefaultName);

            await NewProjectGenController.Instance.UnsafeGenerateProjectAsync(userSelection);

            // Build solution
            var outputPath = Path.Combine(_fixture.TestProjectsPath, projectName);
            var result = GenerationFixture.BuildSolution(projectName, outputPath);

            // Assert
            Assert.True(result.exitCode.Equals(0), $"Solution {targetProjectTemplate.Name} was not built successfully. {Environment.NewLine}Errors found: {GenerationFixture.GetErrorLines(result.outputFile)}.{Environment.NewLine}Please see {Path.GetFullPath(result.outputFile)} for more details.");

            // Clean
            Directory.Delete(outputPath, true);
        }

        [Theory]
        [MemberData("GetProjectTemplates")]
        [Trait("Type", "ProjectGeneration")]
        public async void GenerateAllPagesAndFeaturesRandomNames(string projectType, string framework, string language)
        {
            SetUpFixtureForTesting(language);

            var targetProjectTemplate = GenerationFixture.Templates.FirstOrDefault(t => t.GetTemplateType() == TemplateType.Project
                                                                                     && t.GetProjectTypeList().Contains(projectType)
                                                                                     && t.GetFrameworkList().Contains(framework)
                                                                                     && !t.GetIsHidden()
                                                                                     && t.GetLanguage() == language);
            var projectName = $"{projectType}{framework}AllRandom";

            ProjectName = projectName;
            ProjectPath = Path.Combine(_fixture.TestProjectsPath, projectName, projectName);
            OutputPath = ProjectPath;

            var userSelection = GenerationFixture.SetupProject(projectType, framework, language);

            GenerationFixture.AddItems(userSelection, GenerationFixture.GetTemplates(framework), GenerationFixture.GetRandomName);

            await NewProjectGenController.Instance.UnsafeGenerateProjectAsync(userSelection);

            // Build solution
            var outputPath = Path.Combine(_fixture.TestProjectsPath, projectName);
            var result = GenerationFixture.BuildSolution(projectName, outputPath);

            // Assert
            Assert.True(result.exitCode.Equals(0), $"Solution {targetProjectTemplate.Name} was not built successfully. {Environment.NewLine}Errors found: {GenerationFixture.GetErrorLines(result.outputFile)}.{Environment.NewLine}Please see {Path.GetFullPath(result.outputFile)} for more details.");

            // Clean
            Directory.Delete(outputPath, true);
        }

        public static IEnumerable<object[]> GetProjectTemplates()
        {
            return GenerationFixture.GetProjectTemplates();
        }

        public static IEnumerable<object[]> GetPageAndFeatureTemplates()
        {
            return GenerationFixture.GetPageAndFeatureTemplates();
        }
    }
}
