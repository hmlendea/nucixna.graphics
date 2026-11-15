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
    /// Namespace, using-directive, and compilation-unit checks over the handwritten sources.
    /// </summary>
    [TestFixture]
    public sealed class NamespaceConventionTests
    {
        private static readonly string[] ForbiddenNamespaceSegments = ["Interfaces", "Enums", "Enumerations"];

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingUsingPlacement_ThenAllDirectivesPrecedeTheNamespace(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
                int namespaceStart = root.DescendantNodes()
                    .OfType<BaseNamespaceDeclarationSyntax>()
                    .Single()
                    .GetLocation()
                    .SourceSpan
                    .Start;
                UsingDirectiveSyntax[] misplacedUsings = [.. root.DescendantNodes()
                    .OfType<UsingDirectiveSyntax>()
                    .Where(directive => directive.GetLocation().SourceSpan.Start > namespaceStart)];

                Assert.That(
                    misplacedUsings,
                    Is.Empty,
                    $"{tree.FilePath}: Place all using directives above and outside the namespace.");
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingNamespaces_ThenNoInterfaceOrEnumNamespaceIsUsed(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                BaseNamespaceDeclarationSyntax namespaceDeclaration = tree.GetCompilationUnitRoot()
                    .DescendantNodes()
                    .OfType<BaseNamespaceDeclarationSyntax>()
                    .Single();
                string[] forbiddenSegments = [.. namespaceDeclaration.Name.ToString()
                    .Split('.')
                    .Where(segment => ForbiddenNamespaceSegments.Contains(segment, StringComparer.Ordinal))];

                Assert.That(
                    forbiddenSegments,
                    Is.Empty,
                    $"{tree.FilePath}: Interfaces and enumerations must be colocated with their domain, "
                        + $"never placed in a '{string.Join("' or '", forbiddenSegments)}' namespace.");
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingGlobalUsings_ThenNoneAreDeclared(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
                UsingDirectiveSyntax[] globalUsings = [.. root.Usings
                    .Where(directive => directive.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword))];

                Assert.That(
                    globalUsings,
                    Is.Empty,
                    $"{tree.FilePath}: 'global using' directives are prohibited; add the using to each file that requires it.");
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingFileStructure_ThenOneCompilationUnitPerFileIsDeclared(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

                Assert.That(
                    root.DescendantNodes().OfType<GlobalStatementSyntax>(),
                    Is.Empty,
                    $"{tree.FilePath}: Top-level statements are prohibited; declare an explicit type.");

                Assert.That(
                    root.Members.Count(member => member is BaseNamespaceDeclarationSyntax),
                    Is.EqualTo(1),
                    $"{tree.FilePath}: Declare exactly one namespace per file.");
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingNamespaces_ThenOneBlankLinePrecedesThem(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

                if (root.Usings.Count == 0)
                {
                    continue;
                }

                BaseNamespaceDeclarationSyntax namespaceDeclaration = root.DescendantNodes()
                    .OfType<BaseNamespaceDeclarationSyntax>()
                    .Single();
                int lastUsingLine = root.Usings[^1].GetLocation().GetLineSpan().StartLinePosition.Line;
                int namespaceLine = namespaceDeclaration.GetLocation().GetLineSpan().StartLinePosition.Line;

                Assert.That(
                    namespaceLine - lastUsingLine,
                    Is.EqualTo(2),
                    $"{tree.FilePath}: Separate the final using directive and the namespace with exactly one blank line.");
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingNamespaceMembers_ThenTheyAreIndented(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                BaseNamespaceDeclarationSyntax namespaceDeclaration = tree.GetCompilationUnitRoot()
                    .DescendantNodes()
                    .OfType<BaseNamespaceDeclarationSyntax>()
                    .Single();
                List<string> violations = [];

                foreach (MemberDeclarationSyntax member in namespaceDeclaration.Members)
                {
                    int startColumn = member.GetLocation().GetLineSpan().StartLinePosition.Character;

                    if (startColumn != 4)
                    {
                        violations.Add(
                            $"{tree.FilePath}:{member.GetLocation().GetLineSpan().StartLinePosition.Line + 1}: "
                                + "Indent namespace members by four spaces.");
                    }
                }

                Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
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
    }
}
