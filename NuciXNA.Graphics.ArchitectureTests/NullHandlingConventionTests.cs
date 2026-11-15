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
    /// Null-handling checks over the handwritten sources.
    /// </summary>
    [TestFixture]
    public sealed class NullHandlingConventionTests
    {
        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingNullComparisons_ThenIsNullPatternsAreUsed(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                List<string> violations = [];

                foreach (BinaryExpressionSyntax expression in tree.GetRoot()
                    .DescendantNodes()
                    .OfType<BinaryExpressionSyntax>())
                {
                    if (!expression.IsKind(SyntaxKind.EqualsExpression) &&
                        !expression.IsKind(SyntaxKind.NotEqualsExpression))
                    {
                        continue;
                    }

                    if (expression.Left.IsKind(SyntaxKind.NullLiteralExpression) ||
                        expression.Right.IsKind(SyntaxKind.NullLiteralExpression))
                    {
                        violations.Add(
                            $"{Describe(tree, expression)}: Use 'is null' or 'is not null' instead of comparing with null.");
                    }
                }

                Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingCoalescing_ThenNoNullCoalescingIsUsed(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                BinaryExpressionSyntax[] coalescing = [.. tree.GetRoot()
                    .DescendantNodes()
                    .OfType<BinaryExpressionSyntax>()
                    .Where(expression => expression.IsKind(SyntaxKind.CoalesceExpression))];

                Assert.That(
                    coalescing,
                    Is.Empty,
                    string.Join(
                        Environment.NewLine,
                        [.. coalescing.Select(expression => $"{Describe(tree, expression)}: '??' is prohibited; use explicit control flow or '??=' instead.")]));
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingPatternMatching_ThenNoDiscardDesignationsAreUsed(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                DeclarationPatternSyntax[] discardPatterns = [.. tree.GetRoot()
                    .DescendantNodes()
                    .OfType<DeclarationPatternSyntax>()
                    .Where(pattern => pattern.Designation is DiscardPatternSyntax)];

                Assert.That(
                    discardPatterns,
                    Is.Empty,
                    string.Join(
                        Environment.NewLine,
                        [.. discardPatterns.Select(pattern => $"{Describe(tree, pattern)}: Do not declare a discard in a pattern; declare a meaningful name.")]));
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
