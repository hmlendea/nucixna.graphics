using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using NUnit.Framework;

namespace NuciXNA.Graphics.ArchitectureTests
{
    /// <summary>
    /// Solution file convention checks.
    /// </summary>
    [TestFixture]
    public sealed class SolutionConventionTests
    {
        private const string SolutionPath = "NuciXNA.Graphics.slnx";

        private static readonly string[] ExpectedProjectPaths =
        [
            "NuciXNA.Graphics/NuciXNA.Graphics.csproj",
            "NuciXNA.Graphics.ArchitectureTests/NuciXNA.Graphics.ArchitectureTests.csproj",
            "NuciXNA.Graphics.UnitTests/NuciXNA.Graphics.UnitTests.csproj"
        ];

        public static IEnumerable<TestCaseData> Projects
            => ExpectedProjectPaths.Select(path => new TestCaseData(path).SetName($"{{m}}({path})"));

        [Test]
        public void GivenTheSolution_WhenLoaded_ThenItUsesTheSdkStyleFormat()
        {
            XDocument solution = LoadSolution();
            string content = ReadSolution();

            Assert.That(solution.Root, Is.Not.Null, "The solution file must have a root element.");
            Assert.That(
                solution.Root!.Name.LocalName,
                Is.EqualTo("Solution"),
                "The solution file must use the SDK-style <Solution> root element.");
            Assert.That(
                solution.Root.Attributes(),
                Is.Empty,
                "The <Solution> root element must not declare attributes.");
            Assert.That(
                content.TrimStart(),
                Does.Not.StartWith("<?xml"),
                "The solution file must not declare an XML declaration.");
            Assert.That(
                solution.Descendants().Where(element =>
                    string.Equals(element.Name.LocalName, "Version", StringComparison.Ordinal)),
                Is.Empty,
                "The solution file must not contain a <Version> element.");
        }

        [Test]
        public void GivenTheSolution_WhenLoaded_ThenEveryProjectDeclaresOnlyItsPath()
        {
            XElement[] projects = [.. LoadSolution()
                .Descendants()
                .Where(element => string.Equals(element.Name.LocalName, "Project", StringComparison.Ordinal))];

            Assert.That(projects, Is.Not.Empty, "The solution file must declare projects.");

            foreach (XElement project in projects)
            {
                Assert.That(
                    project.Attributes().Select(attribute => attribute.Name.LocalName),
                    Is.EqualTo(["Path"]),
                    "Each <Project> element must declare only the Path attribute.");
                Assert.That(project.Elements(), Is.Empty, "Each <Project> element must be empty.");
            }
        }

        [TestCaseSource(nameof(Projects))]
        public void GivenTheSolution_WhenCheckingProjects_ThenEveryProjectIsDeclared(string projectPath)
        {
            string[] paths = [.. LoadSolution()
                .Descendants()
                .Where(element => string.Equals(element.Name.LocalName, "Project", StringComparison.Ordinal))
                .Select(project => GetAttributeValue(project, "Path"))];

            Assert.That(
                paths,
                Has.Member(projectPath),
                $"{projectPath}: The project must be declared in the solution.");
        }

        [Test]
        public void GivenTheSolution_WhenCheckingProjects_ThenTheOrderFollowsTheDocumentedConvention()
        {
            string[] paths = [.. LoadSolution()
                .Descendants()
                .Where(element => string.Equals(element.Name.LocalName, "Project", StringComparison.Ordinal))
                .Select(project => GetAttributeValue(project, "Path"))];

            Assert.That(
                paths,
                Is.EqualTo(ExpectedProjectPaths),
                "Order the solution as the main project, then the other source projects, then the test projects.");
        }

        [Test]
        public void GivenTheRepository_WhenCheckingSolutions_ThenOnlySlnxIsUsed()
        {
            Assert.That(
                ArchitectureResources.GetResourcePaths(ArchitectureResources.ProjectFilePrefix)
                    .Where(path => path.EndsWith(".sln", StringComparison.Ordinal)),
                Is.Empty,
                "Use the .slnx solution format, never the legacy .sln format.");
        }

        [TestCaseSource(nameof(Projects))]
        public void GivenTheSolution_WhenCheckingProjects_ThenEachResidesInANamedProjectFolder(string projectPath)
        {
            string projectName = projectPath.Substring(0, projectPath.IndexOf('/'));

            Assert.That(
                projectPath,
                Is.EqualTo($"{projectName}/{projectName}.csproj"),
                "Place every project in a sibling folder named after the project without a src layer.");
        }

        private static XDocument LoadSolution()
            => XDocument.Parse(ReadSolution());

        private static string ReadSolution()
            => ArchitectureResources.ReadResource(ArchitectureResources.ProjectFilePrefix, SolutionPath);

        private static string GetAttributeValue(XElement element, string attributeName)
        {
            XAttribute? attribute = element.Attribute(attributeName);

            if (attribute is null)
            {
                return string.Empty;
            }

            return attribute.Value;
        }
    }
}
