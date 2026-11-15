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
    /// Redundant initialiser and primitive-literal checks over the handwritten sources.
    /// </summary>
    [TestFixture]
    public sealed class RedundantInitialisersConventionTests
    {
        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        public void GivenSources_WhenCheckingFields_ThenNoRedundantInitialisersAreUsed(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                List<string> violations = [];

                foreach (FieldDeclarationSyntax field in tree.GetRoot().DescendantNodes().OfType<FieldDeclarationSyntax>())
                {
                    string modifiers = field.Modifiers.ToString();

                    if (field.Declaration.Variables.Count != 1 ||
                        field.Declaration.Variables[0].Initializer is null)
                    {
                        continue;
                    }

                    if (IsTotalReduction(field.Declaration.Variables[0].Initializer.Value))
                    {
                        violations.Add(
                            $"{Describe(tree, field.Declaration.Variables[0])}: Remove the redundant initialiser '{field.Declaration.Variables[0].Initializer.Value}'; default initialisation is equivalent.");
                    }
                }

                Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingAssignments_ThenNoIncrementBySelfAssignmentIsUsed(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                List<string> violations = [];

                foreach (ExpressionStatementSyntax statement in tree.GetRoot()
                    .DescendantNodes()
                    .OfType<ExpressionStatementSyntax>())
                {
                    if (!IsSelfIncrementOrDecrement(statement.Expression))
                    {
                        continue;
                    }

                    violations.Add(
                        $"{Describe(tree, statement)}: Use += 1 or -= 1 instead of assigning a variable to itself.");
                }

                Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
            }
        }

        private static bool IsSelfIncrementOrDecrement(ExpressionSyntax expression)
        {
            if (expression is not AssignmentExpressionSyntax assignment ||
                !assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
            {
                return false;
            }

            if (assignment.Right is not BinaryExpressionSyntax binary ||
                (!binary.IsKind(SyntaxKind.AddExpression) && !binary.IsKind(SyntaxKind.SubtractExpression)))
            {
                return false;
            }

            bool sameTarget = string.Equals(
                assignment.Left.ToString(),
                binary.Left.ToString(),
                StringComparison.Ordinal);
            bool unitIncrement = binary.Right.ToString() is "1" or "1.0";

            return sameTarget && unitIncrement;
        }

        private static bool IsTotalReduction(ExpressionSyntax initializer)
            => initializer switch
            {
                LiteralExpressionSyntax literal when
                    literal.IsKind(SyntaxKind.FalseLiteralExpression)
                    || literal.IsKind(SyntaxKind.DefaultLiteralExpression)
                    => true,
                LiteralExpressionSyntax literal when
                    literal.IsKind(SyntaxKind.NumericLiteralExpression) &&
                    literal.Token.ValueText == "0"
                    => true,
                LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.NullLiteralExpression)
                    => true,
                DefaultExpressionSyntax => true,
                _ => false
            };

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
