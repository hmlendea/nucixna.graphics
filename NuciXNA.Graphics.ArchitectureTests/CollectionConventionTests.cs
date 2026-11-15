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
    /// Collection checks over the handwritten sources.
    /// </summary>
    [TestFixture]
    public sealed class CollectionConventionTests
    {
        private static readonly string[] CollectionTypes =
        [
            "List<", "IList<", "IReadOnlyList<", "HashSet<", "Dictionary<", "IEnumerable<"
        ];

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingCollectionLiterals_ThenOnlyCollectionExpressionsAreUsed(
            string sourcePrefix)
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

                    if (CollectionTypes.Any(typeName.StartsWith) ||
                        typeName.EndsWith("[]", StringComparison.Ordinal))
                    {
                        violations.Add(
                            $"{Describe(tree, creation)}: Use a collection expression '[]' instead of '{typeName}(...)'.");
                    }
                }

                Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        public void GivenSources_WhenCheckingMemberTypes_ThenCollectionsUseEnumerable(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                List<string> violations = [];

                foreach (PropertyDeclarationSyntax property in tree.GetRoot()
                    .DescendantNodes()
                    .OfType<PropertyDeclarationSyntax>())
                {
                    violations.AddRange(CheckMemberType(tree, property, property.Type));
                }

                foreach (EventFieldDeclarationSyntax field in tree.GetRoot()
                    .DescendantNodes()
                    .OfType<EventFieldDeclarationSyntax>())
                {
                    violations.AddRange(CheckMemberType(tree, field, field.Declaration.Type));
                }

                Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
            }
        }

        private static List<string> CheckMemberType(
            SyntaxTree tree,
            MemberDeclarationSyntax member,
            TypeSyntax? type)
        {
            List<string> violations = [];

            if (type is not null && IsRelaxedCollectionType(type.ToString()) &&
                member.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PublicKeyword)))
            {
                violations.Add(
                    $"{Describe(tree, member)}: Public collection members must expose IEnumerable<T>, not '{type}'.");
            }

            return violations;
        }

        private static bool IsRelaxedCollectionType(string typeName)
            => typeName switch
            {
                var name => name.StartsWith("List<", StringComparison.Ordinal) ||
                    name.StartsWith("IList<", StringComparison.Ordinal) ||
                    name.StartsWith("IReadOnlyList<", StringComparison.Ordinal) ||
                    name.StartsWith("HashSet<", StringComparison.Ordinal) ||
                    name.StartsWith("Dictionary<", StringComparison.Ordinal)
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
