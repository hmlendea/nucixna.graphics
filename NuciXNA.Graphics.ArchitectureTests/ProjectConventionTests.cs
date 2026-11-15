using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using NUnit.Framework;

namespace NuciXNA.Graphics.ArchitectureTests
{
    /// <summary>
    /// Project convention checks over the embedded project files.
    /// </summary>
    [TestFixture]
    public sealed class ProjectConventionTests
    {
        private static readonly string ProductionProjectPath = "NuciXNA.Graphics/NuciXNA.Graphics.csproj";

        private static readonly string UnitTestProjectPath = "NuciXNA.Graphics.UnitTests/NuciXNA.Graphics.UnitTests.csproj";

        private static readonly string ArchitectureTestProjectPath =
            "NuciXNA.Graphics.ArchitectureTests/NuciXNA.Graphics.ArchitectureTests.csproj";

        /// <summary>
        /// The production and unit-test projects pre-date the nullable-reference convention and still expose
        /// unannotated public members; enabling it there surfaces over 130 pre-existing warnings. These two projects
        /// are the only narrowly scoped exemption until the nullable migration is delivered as its own change.
        /// </summary>
        private static readonly string[] NullableExemptions =
        [
            ProductionProjectPath,
            UnitTestProjectPath
        ];

        public static IEnumerable<TestCaseData> Projects
            => new[]
            {
                ProductionProjectPath,
                UnitTestProjectPath,
                ArchitectureTestProjectPath
            }
                .Select(path => new TestCaseData(path).SetName($"{{m}}({path})"));

        public static IEnumerable<TestCaseData> TestProjects
            => new[]
            {
                UnitTestProjectPath,
                ArchitectureTestProjectPath
            }
                .Select(path => new TestCaseData(path).SetName($"{{m}}({path})"));

        [TestCaseSource(nameof(Projects))]
        public void GivenAProject_WhenCheckingMetadata_ThenTheSharedBuildPropertiesAreDeclared(string projectPath)
        {
            XDocument project = LoadProject(projectPath);

            Assert.That(
                GetProperty(project, "TargetFramework"),
                Is.EqualTo("net10.0"),
                $"{projectPath}: The target framework must be net10.0.");
            Assert.That(
                GetProperty(project, "ImplicitUsings"),
                Is.EqualTo("disable"),
                $"{projectPath}: Implicit usings must be disabled.");
            Assert.That(
                GetProperty(project, "LangVersion"),
                Is.EqualTo("latest"),
                $"{projectPath}: The latest language version must be used.");
        }

        [TestCaseSource(nameof(Projects))]
        public void GivenAProject_WhenCheckingNullableMetadata_ThenItIsEnabledOrExplicitlyExempt(string projectPath)
        {
            string nullable = GetProperty(LoadProject(projectPath), "Nullable");

            if (NullableExemptions.Contains(projectPath, StringComparer.Ordinal))
            {
                Assert.That(
                    nullable,
                    Is.Not.EqualTo("enable"),
                    $"{projectPath}: Remove the project from the documented nullable exemption once the migration ships.");
                return;
            }

            Assert.That(nullable, Is.EqualTo("enable"), $"{projectPath}: Nullable references must be enabled.");
        }

        [TestCaseSource(nameof(Projects))]
        public void GivenAProject_WhenCheckingPackages_ThenReferencesAreAlphabetical(string projectPath)
        {
            XDocument project = LoadProject(projectPath);
            string[] packages = [.. project.Descendants("PackageReference")
                .Select(reference => GetAttributeValue(reference, "Include"))];

            Assert.That(packages, Is.Not.Empty, $"{projectPath}: No package references were found.");
            Assert.That(
                packages,
                Is.EqualTo(packages.OrderBy(package => package, StringComparer.Ordinal).ToArray()),
                $"{projectPath}: Package references must be ordered alphabetically.");
        }

        [TestCaseSource(nameof(TestProjects))]
        public void GivenATestProject_WhenCheckingMetadata_ThenTheTestFlagsAreDeclared(string projectPath)
        {
            XDocument project = LoadProject(projectPath);

            Assert.That(
                GetProperty(project, "IsTestProject"),
                Is.EqualTo("true"),
                $"{projectPath}: Declare IsTestProject.");
            Assert.That(
                GetProperty(project, "ExcludeFromCodeCoverage"),
                Is.EqualTo("true"),
                $"{projectPath}: Declare ExcludeFromCodeCoverage.");
            Assert.That(
                GetProperty(project, "IsPackable"),
                Is.EqualTo("false"),
                $"{projectPath}: Tests must not be packable.");
        }

        [Test]
        public void GivenTheProductionProject_WhenCheckingPackaging_ThenItIsPackable()
        {
            XDocument project = LoadProject(ProductionProjectPath);

            Assert.That(
                GetProperty(project, "IsPackable"),
                Is.EqualTo("true"),
                "The production project is published as a NuGet package and must be packable.");
        }

        [Test]
        public void GivenTheProductionProject_WhenCheckingReferences_ThenNoTestProjectIsReferenced()
        {
            string[] references = GetProjectReferences(LoadProject(ProductionProjectPath));

            Assert.That(
                references.Where(reference => reference.Contains("Tests", StringComparison.OrdinalIgnoreCase)),
                Is.Empty,
                "The production project must not reference test projects.");
        }

        [TestCase("../NuciXNA.Graphics/NuciXNA.Graphics.csproj")]
        public void GivenTheUnitTestProject_WhenCheckingReferences_ThenTheProductionProjectIsReferenced(string expected)
        {
            string[] references = GetProjectReferences(LoadProject(UnitTestProjectPath));

            Assert.That(
                references,
                Has.Member(expected),
                "The unit test project must reference the production project.");
        }

        [Test]
        public void GivenTheArchitectureTestProject_WhenCheckingReferences_ThenTheProductionProjectIsReferenced()
        {
            string[] references = GetProjectReferences(LoadProject(ArchitectureTestProjectPath));

            Assert.That(
                references,
                Has.Member("../NuciXNA.Graphics/NuciXNA.Graphics.csproj"),
                "The architecture test project must reference the production project.");
        }

        [Test]
        public void GivenTheProductionProject_WhenCheckingReferences_ThenNoInternalsVisibleToIsDeclared()
        {
            XDocument project = LoadProject(ProductionProjectPath);

            Assert.That(
                project.Descendants().Where(element =>
                    string.Equals(element.Name.LocalName, "InternalsVisibleTo", StringComparison.Ordinal)),
                Is.Empty,
                "The production project must not declare InternalsVisibleTo.");
        }

        private static XDocument LoadProject(string relativePath)
            => XDocument.Parse(
                ArchitectureResources.ReadResource(
                    ArchitectureResources.ProjectFilePrefix,
                    relativePath));

        private static string[] GetProjectReferences(XDocument project)
            => [.. project.Descendants("ProjectReference")
                .Select(reference => GetAttributeValue(reference, "Include").Replace('\\', '/'))];

        private static string GetAttributeValue(XElement element, string attributeName)
        {
            XAttribute? attribute = element.Attribute(attributeName);

            if (attribute is null)
            {
                return string.Empty;
            }

            return attribute.Value;
        }

        private static string GetProperty(XDocument project, string name)
        {
            IEnumerable<XElement> elements = project.Descendants()
                .Where(element => string.Equals(element.Name.LocalName, name, StringComparison.Ordinal));
            XElement? first = elements.FirstOrDefault();

            if (first is null)
            {
                return string.Empty;
            }

            return first.Value;
        }
    }
}
