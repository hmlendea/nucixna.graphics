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
    /// Indentation and whitespace checks over the handwritten sources.
    /// </summary>
    [TestFixture]
    public sealed class IndentationConventionTests
    {
        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingIndentation_ThenTabsAreNeverUsed(string sourcePrefix)
        {
            foreach (string source in CollectSources(sourcePrefix))
            {
                Assert.That(
                    ReadSource(sourcePrefix, source),
                    Does.Not.Contain('\t'),
                    $"Violated convention in {source}: indentation must use four spaces, never tabs.");
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingTrailingWhitespace_ThenNoneExists(string sourcePrefix)
        {
            foreach (string source in CollectSources(sourcePrefix))
            {
                Assert.That(
                    ReadSource(sourcePrefix, source),
                    Does.Not.Match(@"[ \t]+(\r?\n)"),
                    $"Violated convention in {source}: trailing whitespace is not allowed.");
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingBlankLines_ThenAtMostOneConsecutiveBlankLineExists(string sourcePrefix)
        {
            foreach (string source in CollectSources(sourcePrefix))
            {
                Assert.That(
                    ReadSource(sourcePrefix, source).Replace("\r", string.Empty, StringComparison.Ordinal),
                    Does.Not.Contain("\n\n\n"),
                    $"Violated convention in {source}: two or more consecutive blank lines are prohibited.");
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingClosingDelimiters_ThenNoBlankLinePrecedesThem(string sourcePrefix)
        {
            foreach (string source in CollectSources(sourcePrefix))
            {
                Assert.That(
                    ReadSource(sourcePrefix, source),
                    Does.Not.Match(@"\n[ \t]*\n[ \t]*[}\])]"),
                    $"Violated convention in {source}: no blank line may immediately precede a closing delimiter.");
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingFileEndings_ThenEachEndsWithAnEmptyLine(string sourcePrefix)
        {
            foreach (string source in CollectSources(sourcePrefix))
            {
                Assert.That(
                    ReadSource(sourcePrefix, source),
                    Does.EndWith(Environment.NewLine),
                    $"Violated convention in {source}: every file must end with an empty line.");
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingIndentWidth_ThenMultiplesOfFourSpacesAreUsed(string sourcePrefix)
        {
            foreach (string source in CollectSources(sourcePrefix))
            {
                string[] lines = ReadSource(sourcePrefix, source)
                    .Replace("\r\n", "\n", StringComparison.Ordinal)
                    .Split('\n');

                for (int lineIndex = 0; lineIndex < lines.Length; lineIndex += 1)
                {
                    string line = lines[lineIndex];
                    int indentation = line.Length - line.TrimStart().Length;

                    Assert.That(
                        indentation % 4,
                        Is.Zero,
                        $"Violated convention in {source}:{lineIndex + 1}: indentation must be a multiple of four spaces.");
                }
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingBraces_ThenOpeningBracesOccupyTheirOwnLines(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                List<string> violations = [];

                foreach (SyntaxNode node in tree.GetRoot().DescendantNodes())
                {
                    if (!EndsWithInlineOpeningBrace(node))
                    {
                        continue;
                    }

                    violations.Add(
                        $"{tree.FilePath}:{node.GetLocation().GetLineSpan().EndLinePosition.Line + 1}: "
                            + "Place the opening brace on its own line.");
                }

                Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingElseIf_ThenItIsKeptOnASingleLine(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                ElseClauseSyntax[] splitElseIfs = [.. tree.GetRoot()
                    .DescendantNodes()
                    .OfType<ElseClauseSyntax>()
                    .Where(clause => clause.Statement is IfStatementSyntax &&
                        clause.ElseKeyword.GetLocation().GetLineSpan().EndLinePosition.Line !=
                        clause.Statement.GetLocation().GetLineSpan().StartLinePosition.Line)];

                Assert.That(
                    splitElseIfs,
                    Is.Empty,
                    $"{tree.FilePath}: Keep 'else if' on a single line.");
            }
        }

        private static bool EndsWithInlineOpeningBrace(SyntaxNode node)
        {
            SyntaxToken openBrace = node switch
            {
                BaseTypeDeclarationSyntax declaration => declaration.OpenBraceToken,
                NamespaceDeclarationSyntax declaration => declaration.OpenBraceToken,
                MethodDeclarationSyntax method when method.Body is not null => method.Body.OpenBraceToken,
                ConstructorDeclarationSyntax constructor when constructor.Body is not null => constructor.Body.OpenBraceToken,
                DestructorDeclarationSyntax destructor when destructor.Body is not null => destructor.Body.OpenBraceToken,
                PropertyDeclarationSyntax property => GetAccessorOpenBrace(property),                AccessorDeclarationSyntax accessor when accessor.Body is not null => accessor.Body.OpenBraceToken,
                IfStatementSyntax statement when statement.Statement is BlockSyntax block => block.OpenBraceToken,
                ElseClauseSyntax clause when clause.Statement is BlockSyntax block => block.OpenBraceToken,
                ForStatementSyntax statement when statement.Statement is BlockSyntax block => block.OpenBraceToken,
                CommonForEachStatementSyntax statement when statement.Statement is BlockSyntax block => block.OpenBraceToken,
                WhileStatementSyntax statement when statement.Statement is BlockSyntax block => block.OpenBraceToken,
                DoStatementSyntax statement when statement.Statement is BlockSyntax block => block.OpenBraceToken,
                UsingStatementSyntax statement when statement.Statement is BlockSyntax block => block.OpenBraceToken,
                LockStatementSyntax statement when statement.Statement is BlockSyntax block => block.OpenBraceToken,
                _ => default
            };

            if (openBrace.IsKind(SyntaxKind.None) || IsInlineEmptyBody(openBrace))
            {
                return false;
            }

            return openBrace.GetLocation().GetLineSpan().StartLinePosition.Line ==
                node.GetLocation().GetLineSpan().StartLinePosition.Line;
        }

        /// <summary>
        /// Empty bodies are conventionally written as '{ }' on the declaration line and are not control-flow
        /// bodies, so the own-line brace rule does not apply to them.
        /// </summary>
        private static bool IsInlineEmptyBody(SyntaxToken openBrace)
            => openBrace.Parent is BlockSyntax block && block.Statements.Count == 0;

        private static SyntaxToken GetAccessorOpenBrace(PropertyDeclarationSyntax property)
        {
            AccessorListSyntax? accessorList = property.AccessorList;

            if (accessorList is null ||
                accessorList.Accessors.All(accessor => accessor.Body is null))
            {
                return default;
            }

            return accessorList.OpenBraceToken;
        }

        private static string ReadSource(string sourcePrefix, string source)
            => ArchitectureResources.ReadResource(sourcePrefix, source);

        private static IEnumerable<string> CollectSources(string sourcePrefix)
        {
            IReadOnlyList<string> sources = ArchitectureResources.GetResourcePaths(sourcePrefix);

            Assert.That(
                sources,
                Is.Not.Empty,
                $"Violated convention: no source files were found under '{sourcePrefix}'.");

            return sources;
        }

        private static IEnumerable<SyntaxTree> CollectParseTrees(string sourcePrefix)
            => CollectSources(sourcePrefix)
                .Select(source => CSharpSyntaxTree.ParseText(ReadSource(sourcePrefix, source), path: source));
    }
}
