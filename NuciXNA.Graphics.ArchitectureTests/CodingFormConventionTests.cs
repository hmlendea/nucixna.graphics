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
    /// Statically decidable coding-form checks over the handwritten sources.
    /// </summary>
    [TestFixture]
    public sealed class CodingFormConventionTests
    {
        private const int MaximumParameterCount = 7;

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingSelfAssignment_ThenCompoundAssignmentIsUsed(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                List<string> violations = [];

                foreach (AssignmentExpressionSyntax assignment in tree.GetRoot()
                    .DescendantNodes()
                    .OfType<AssignmentExpressionSyntax>())
                {
                    if (!assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
                    {
                        continue;
                    }

                    if (assignment.Right is not BinaryExpressionSyntax binary)
                    {
                        continue;
                    }

                    if (!binary.IsKind(SyntaxKind.AddExpression) &&
                        !binary.IsKind(SyntaxKind.SubtractExpression))
                    {
                        continue;
                    }

                    if (string.Equals(assignment.Left.ToString(), binary.Left.ToString(), StringComparison.Ordinal))
                    {
                        violations.Add(
                            $"{Describe(tree, assignment)}: Use the compound assignment form instead of assigning a variable to itself.");
                    }
                }

                Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingLambdas_ThenParameterLimitsAreRespected(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                List<string> violations = [];

                foreach (LambdaExpressionSyntax lambda in tree.GetRoot().DescendantNodes().OfType<LambdaExpressionSyntax>())
                {
                    int parameterCount = lambda switch
                    {
                        ParenthesizedLambdaExpressionSyntax parenthesized => parenthesized.ParameterList.Parameters.Count,
                        SimpleLambdaExpressionSyntax _ => 1,
                        _ => 0
                    };

                    if (parameterCount > MaximumParameterCount)
                    {
                        violations.Add(
                            $"{Describe(tree, lambda)}: Lambdas must declare at most {MaximumParameterCount} parameters.");
                    }
                }

                Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingCatchBlocks_ThenNoneAreEmpty(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                CatchClauseSyntax[] emptyCatchBlocks = [.. tree.GetRoot()
                    .DescendantNodes()
                    .OfType<CatchClauseSyntax>()
                    .Where(clause => clause.Block is not null && clause.Block.Statements.Count == 0)];

                Assert.That(
                    emptyCatchBlocks,
                    Is.Empty,
                    string.Join(
                        Environment.NewLine,
                        [.. emptyCatchBlocks.Select(clause => $"{Describe(tree, clause)}: Do not leave an empty catch block; convert the failure to an explicit safe result or a sanitised exception.")]));
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingConditionals_ThenNoneAreNested(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                List<string> violations = [];

                foreach (ConditionalExpressionSyntax conditional in tree.GetRoot()
                    .DescendantNodes()
                    .OfType<ConditionalExpressionSyntax>())
                {
                    if (conditional.Ancestors().OfType<ConditionalExpressionSyntax>().Any())
                    {
                        violations.Add(
                            $"{Describe(tree, conditional)}: Do not nest conditional expressions; extract a named helper.");
                    }
                }

                Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingOverloads_ThenTheyAreAdjacent(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                List<string> violations = [];

                foreach (TypeDeclarationSyntax typeDeclaration in tree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>())
                {
                    MethodDeclarationSyntax[] methods = [.. typeDeclaration.Members.OfType<MethodDeclarationSyntax>()];

                    foreach (IGrouping<string, MethodDeclarationSyntax> overloadGroup in methods
                        .Where(method => !method.Modifiers.Any(SyntaxKind.OverrideKeyword))
                        .ToLookup(method => method.Identifier.ValueText + "|" + GetAccessibility(method))
                        .Where(group => group.Count() > 1))
                    {
                        int[] indices = [.. overloadGroup.Select(overload => Array.IndexOf(methods, overload))];

                        if (indices.Max() - indices.Min() != indices.Length - 1)
                        {
                            violations.Add(
                                $"{Describe(tree, overloadGroup.First())}: Group the '{overloadGroup.Key}' overloads together.");
                        }
                    }
                }

                Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingRegexUsage_ThenPatternsAreReused(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                List<string> violations = [];

                foreach (ObjectCreationExpressionSyntax creation in tree.GetRoot()
                    .DescendantNodes()
                    .OfType<ObjectCreationExpressionSyntax>())
                {
                    if (!string.Equals(creation.Type.ToString(), "Regex", StringComparison.Ordinal) &&
                        !string.Equals(creation.Type.ToString(), "System.Text.RegularExpressions.Regex", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (creation.Ancestors().OfType<FieldDeclarationSyntax>().Any())
                    {
                        continue;
                    }

                    violations.Add(
                        $"{Describe(tree, creation)}: Retain and reuse a Regex instance or use GeneratedRegex instead of constructing one per call.");
                }

                Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingInterfaces_ThenParameterNamesMirrorTheImplementation(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                List<string> violations = [];

                foreach (MethodDeclarationSyntax method in tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
                {
                    if (!method.Modifiers.Any(SyntaxKind.OverrideKeyword))
                    {
                        continue;
                    }

                    ClassDeclarationSyntax? declaringType = method.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();

                    if (declaringType is null)
                    {
                        continue;
                    }

                    foreach (InterfaceDeclarationSyntax interfaceDeclaration in CollectInterfaceContracts(sourcePrefix, declaringType))
                    {
                        MethodDeclarationSyntax? contract = interfaceDeclaration.Members
                            .OfType<MethodDeclarationSyntax>()
                            .FirstOrDefault(candidate => string.Equals(
                                candidate.Identifier.ValueText,
                                method.Identifier.ValueText,
                                StringComparison.Ordinal));

                        if (contract is null)
                        {
                            continue;
                        }

                        string expected = string.Join(",", contract.ParameterList.Parameters.Select(parameter => parameter.Identifier.ValueText));
                        string actual = string.Join(",", method.ParameterList.Parameters.Select(parameter => parameter.Identifier.ValueText));

                        if (!string.Equals(expected, actual, StringComparison.Ordinal))
                        {
                            violations.Add(
                                $"{Describe(tree, method)}: Interface and implementation parameter names must match exactly.");
                        }
                    }
                }

                Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
            }
        }

        private static string GetAccessibility(MethodDeclarationSyntax method)
        {
            foreach (SyntaxToken modifier in method.Modifiers)
            {
                if (modifier.IsKind(SyntaxKind.PublicKeyword) ||
                    modifier.IsKind(SyntaxKind.ProtectedKeyword) ||
                    modifier.IsKind(SyntaxKind.InternalKeyword) ||
                    modifier.IsKind(SyntaxKind.PrivateKeyword))
                {
                    return modifier.Text;
                }
            }

            return "private";
        }

        private static IEnumerable<InterfaceDeclarationSyntax> CollectInterfaceContracts(
            string sourcePrefix,
            ClassDeclarationSyntax classDeclaration)
        {
            string[] implementedNames = [];

            if (classDeclaration.BaseList is not null)
            {
                implementedNames = [.. classDeclaration.BaseList.Types
                    .Select(baseType => baseType.Type.ToString().Split('<')[0])];
            }

            return CollectParseTrees(sourcePrefix)
                .SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<InterfaceDeclarationSyntax>())
                .Where(interfaceDeclaration => implementedNames.Contains(
                    interfaceDeclaration.Identifier.ValueText,
                    StringComparer.Ordinal));
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
