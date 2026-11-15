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
    /// Type-conversion checks over the handwritten sources.
    /// </summary>
    [TestFixture]
    public sealed class TypeHandlingConventionTests
    {
        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingParameters_ThenNoOutOrRefParametersAreDeclared(
            string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                List<string> violations = [];

                foreach (ParameterSyntax parameter in tree.GetRoot().DescendantNodes().OfType<ParameterSyntax>())
                {
                    if (parameter.Modifiers.Any(SyntaxKind.RefKeyword) ||
                        parameter.Modifiers.Any(SyntaxKind.OutKeyword))
                    {
                        violations.Add(
                            $"{Describe(tree, parameter)}: Ref and 'out' parameters are prohibited; return a dedicated type instead.");
                    }
                }

                Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        public void GivenSources_WhenCheckingLiterals_ThenNoZeroPrimitiveCoordinatesAreUsed(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                List<string> violations = [];

                foreach (ObjectCreationExpressionSyntax creation in tree.GetRoot()
                    .DescendantNodes()
                    .OfType<ObjectCreationExpressionSyntax>())
                {
                    TypeSyntax? creationType = creation.Type;
                    string typeName = string.Empty;

                    if (creationType is not null)
                    {
                        typeName = creationType.ToString();
                    }

                    if (!typeName.EndsWith("Point2D", StringComparison.Ordinal) &&
                        !typeName.EndsWith("Size2D", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    SeparatedSyntaxList<ArgumentSyntax> arguments = default;

                    if (creation.ArgumentList is not null)
                    {
                        arguments = creation.ArgumentList.Arguments;
                    }
                    bool isZeroPoint = arguments.Count >= 2 &&
                        arguments[0].ToString().Trim() == "0" &&
                        arguments[1].ToString().Trim() == "0";

                    if (isZeroPoint)
                    {
                        violations.Add(
                            $"{Describe(tree, creation)}: Use 'Point2D.Empty' or 'Size2D.Zero' instead of 'new {typeName}(0, 0)'.");
                    }
                }

                Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingQualifiedTypeNames_ThenNoInlineFrameworkTypeNamesAreUsed(
            string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                List<string> violations = [];

                foreach (IdentifierNameSyntax identifier in tree.GetRoot()
                    .DescendantNodes()
                    .OfType<IdentifierNameSyntax>())
                {
                    string text = identifier.ToString();

                    if ((text.StartsWith("System.") || text.StartsWith("Microsoft.")) &&
                        !IsRestrictedExpression(identifier))
                    {
                        violations.Add(
                            $"{Describe(tree, identifier)}: Import the namespace and use the short name instead of '{text}'.");
                    }
                }

                Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingQualifiers_ThenNoThisPrefixIsUsed(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                List<string> violations = [];

                foreach (MemberAccessExpressionSyntax access in tree.GetRoot()
                    .DescendantNodes()
                    .OfType<MemberAccessExpressionSyntax>())
                {
                    if (access.Expression is not ThisExpressionSyntax)
                    {
                        continue;
                    }

                    violations.Add(
                        $"{Describe(tree, access)}: Access the member implicitly instead of prefixing it with 'this.'.");
                }

                Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingConsoleUsage_ThenNoConsoleCallsAreUsed(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                InvocationExpressionSyntax[] consoleCalls = [.. tree.GetRoot()
                    .DescendantNodes()
                    .OfType<InvocationExpressionSyntax>()
                    .Where(invocation => invocation.Expression is MemberAccessExpressionSyntax member &&
                        member.Expression is IdentifierNameSyntax name &&
                        name.Identifier.ValueText == "Console")];

                Assert.That(
                    consoleCalls,
                    Is.Empty,
                    string.Join(
                        Environment.NewLine,
                        [.. consoleCalls.Select(expression => $"{Describe(tree, expression)}: Do not write to the console. Use the logging abstractions instead.")]));
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingStrings_ThenCharacterOverloadsAreUsed(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                List<string> violations = [];

                foreach (InvocationExpressionSyntax invocation in tree.GetRoot()
                    .DescendantNodes()
                    .OfType<InvocationExpressionSyntax>())
                {
                    if (invocation.Expression is not MemberAccessExpressionSyntax member)
                    {
                        continue;
                    }

                    string methodName = member.Name.ToString();

                    if (methodName != "StartsWith" && methodName != "EndsWith")
                    {
                        continue;
                    }

                    ArgumentListSyntax arguments = invocation.ArgumentList;
                    if (arguments.Arguments.Count != 1 ||
                        arguments.Arguments[0].Expression is not LiteralExpressionSyntax literal ||
                        !literal.IsKind(SyntaxKind.StringLiteralExpression) ||
                        literal.Token.ValueText.Length != 1)
                    {
                        continue;
                    }

                    violations.Add(
                        $"{Describe(tree, invocation)}: Use 'StartsWith(char)' / 'EndsWith(char)' for single-character checks.");
                }

                Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
            }
        }

        private static bool IsRestrictedExpression(IdentifierNameSyntax identifier)
            => identifier.Parent is MemberAccessExpressionSyntax ||
                identifier.Parent is NamespaceDeclarationSyntax ||
                identifier.Parent is UsingDirectiveSyntax;

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
