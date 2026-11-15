using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using NUnit.Framework;

namespace NuciXNA.Graphics.ArchitectureTests
{
    /// <summary>
    /// Test-source conventions for the unit-test project.
    /// </summary>
    [TestFixture]
    public sealed class TestSourceConventionTests
    {
        [TestCaseSource(nameof(FixtureNames))]
        public void GivenTheUnitTests_WhenCheckingFixtureNames_ThenEachEndsWithTheTestsSuffix(string fixtureName)
        {
            Assert.That(fixtureName, Does.EndWith("Tests"), fixtureName);
        }

        [Test]
        public void GivenTheUnitTests_WhenCheckingSetUp_ThenMethodNamesAreSetUpOrTearDown()
        {
            foreach (SyntaxTree tree in CollectParseTrees(ArchitectureResources.UnitTestSourcePrefix))
            {
                MethodDeclarationSyntax[] lifecycleMethods = [.. tree.GetRoot()
                    .DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()];

                foreach (MethodDeclarationSyntax method in lifecycleMethods)
                {
                    bool hasLifecycleAttribute = method.AttributeLists
                        .SelectMany(list => list.Attributes)
                        .Any(attribute =>
                            attribute.Name.ToString().Contains("SetUp") ||
                            attribute.Name.ToString().Contains("TearDown"));

                    if (!hasLifecycleAttribute)
                    {
                        continue;
                    }

                    string methodName = method.Identifier.ValueText;

                    Assert.That(
                        methodName is "SetUp" or "TearDown",
                        Is.True,
                        $"{Describe(tree, method)}: Lifecycle methods must be named 'SetUp' or 'TearDown'.");
                }
            }
        }

        public static IEnumerable<TestCaseData> FixtureNames()
            => CollectClassNames(ArchitectureResources.UnitTestSourcePrefix)
                .Select(name => new TestCaseData(name));

        private static IEnumerable<string> CollectClassNames(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                foreach (ClassDeclarationSyntax classDeclaration in tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
                {
                    yield return classDeclaration.Identifier.ValueText;
                }
            }
        }

        private static IEnumerable<SyntaxTree> CollectParseTrees(string sourcePrefix)
        {
            IReadOnlyList<string> sources = ArchitectureResources.GetResourcePaths(sourcePrefix);

            Assert.That(
                sources,
                Is.Not.Empty,
                $"Violated convention: no source files were found under '{sourcePrefix}'.");

            return sources.Select(source => CSharpSyntaxTree.ParseText(
                ArchitectureResources.ReadResource(sourcePrefix, source),
                path: source));
        }

        private static string Describe(SyntaxTree tree, SyntaxNode node)
            => $"{tree.FilePath}:{node.GetLocation().GetLineSpan().StartLinePosition.Line + 1}";
    }
}
