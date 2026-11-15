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
    /// Member kind and accessibility ordering over the handwritten sources.
    /// </summary>
    [TestFixture]
    public sealed class MemberOrderConventionTests
    {
        private static readonly string[] AccessibilityRanks =
        [
            "public", "protected", "internal", "private"
        ];

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingFields_ThenTheDocumentedOrderingIsUsed(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                List<string> violations = [];

                foreach (TypeDeclarationSyntax typeDeclaration in tree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>())
                {
                    FieldDeclarationSyntax[] fields = [.. typeDeclaration.Members.OfType<FieldDeclarationSyntax>()];

                    if (fields.Length < 2)
                    {
                        continue;
                    }

                    int previousFieldRank = -1;

                    foreach (FieldDeclarationSyntax field in fields)
                    {
                        int fieldRank = GetFieldRank(field);

                        if (fieldRank < previousFieldRank)
                        {
                            violations.Add(
                                $"{Describe(tree, field)}: Order fields static-readonly → static mutable → instance readonly → instance mutable.");
                        }

                        previousFieldRank = Math.Max(previousFieldRank, fieldRank);
                    }

                    int previousKindRank = -1;

                    foreach (MemberDeclarationSyntax member in typeDeclaration.Members.Where(IsOrderedMember))
                    {
                        int kindRank = GetKindRank(member);

                        if (kindRank < previousKindRank)
                        {
                            violations.Add(
                                $"{Describe(tree, member)}: Order members fields → properties → events → constructors → methods.");
                        }

                        previousKindRank = Math.Max(previousKindRank, kindRank);
                    }
                }

                Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingMembers_ThenAccessibilityOrderIsRespectedWithinEachKind(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                List<string> violations = [];

                foreach (TypeDeclarationSyntax typeDeclaration in tree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>())
                {
                    Dictionary<string, int> previousAccessibilityRank =
                        new()
                        {
                            ["field"] = -1,
                            ["property"] = -1,
                            ["event"] = -1,
                            ["constructor"] = -1,
                            ["method"] = -1
                        };

                    foreach (MemberDeclarationSyntax member in typeDeclaration.Members.Where(IsOrderedMember))
                    {
                        string group = GetKindName(member);
                        string accessibility = GetFirstAccessibilityKeyword(member);
                        int rank = GetAccessibilityRank(accessibility);

                        if (rank < previousAccessibilityRank[group])
                        {
                            violations.Add(
                                $"{Describe(tree, member)}: Order members public → protected → internal → private within each kind (group: {group}).");
                        }

                        previousAccessibilityRank[group] = Math.Max(previousAccessibilityRank[group], rank);
                    }
                }

                Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingOverloads_ThenParameterCountsAscend(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                List<string> violations = [];

                foreach (TypeDeclarationSyntax typeDeclaration in tree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>())
                {
                    ILookup<string, MethodDeclarationSyntax> overloads =
                        typeDeclaration.Members.OfType<MethodDeclarationSyntax>()
                            .ToLookup(method => method.Identifier.ValueText);

                    foreach (IGrouping<string, MethodDeclarationSyntax> overloadGroup in overloads
                        .Where(group => group.Count() > 1))
                    {
                        int previousParameterCount = -1;

                        foreach (MethodDeclarationSyntax overload in overloadGroup)
                        {
                            int parameterCount = overload.ParameterList.Parameters.Count;

                            if (parameterCount < previousParameterCount)
                            {
                                violations.Add(
                                    $"{Describe(tree, overload)}: Order overloads fewest → most parameters.");
                            }

                            previousParameterCount = parameterCount;
                        }
                    }
                }

                Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
            }
        }

        private static int GetFieldRank(FieldDeclarationSyntax field)
        {
            string modifiers = field.Modifiers.ToString();
            bool isConst = field.Modifiers.Any(SyntaxKind.ConstKeyword);
            bool isStatic = modifiers.Contains("static") || isConst;
            bool isReadOnly = modifiers.Contains("readonly") || isConst;

            if (isStatic && isReadOnly)
            {
                return 0;
            }

            if (isStatic)
            {
                return 1;
            }

            if (isReadOnly)
            {
                return 2;
            }

            return 3;
        }

        private static int GetKindRank(MemberDeclarationSyntax member)
            => member switch
            {
                FieldDeclarationSyntax => 0,
                PropertyDeclarationSyntax or IndexerDeclarationSyntax => 1,
                EventFieldDeclarationSyntax or EventDeclarationSyntax => 2,
                ConstructorDeclarationSyntax or DestructorDeclarationSyntax => 3,
                _ => 4
            };

        private static string GetKindName(MemberDeclarationSyntax member)
            => member switch
            {
                FieldDeclarationSyntax => "field",
                PropertyDeclarationSyntax or IndexerDeclarationSyntax => "property",
                EventFieldDeclarationSyntax or EventDeclarationSyntax => "event",
                ConstructorDeclarationSyntax or DestructorDeclarationSyntax => "constructor",
                _ => "method"
            };

        private static bool IsOrderedMember(MemberDeclarationSyntax member)
            => member is FieldDeclarationSyntax ||
                member is PropertyDeclarationSyntax ||
                member is IndexerDeclarationSyntax ||
                member is EventFieldDeclarationSyntax ||
                member is EventDeclarationSyntax ||
                member is ConstructorDeclarationSyntax ||
                member is DestructorDeclarationSyntax ||
                member is MethodDeclarationSyntax;

        private static string GetFirstAccessibilityKeyword(MemberDeclarationSyntax member)
        {
            SyntaxTokenList modifiers = member switch
            {
                FieldDeclarationSyntax field => field.Modifiers,
                PropertyDeclarationSyntax property => property.Modifiers,
                EventFieldDeclarationSyntax eventField => eventField.Modifiers,
                ConstructorDeclarationSyntax constructor => constructor.Modifiers,
                MethodDeclarationSyntax method => method.Modifiers,
                DestructorDeclarationSyntax destructor => destructor.Modifiers,
                _ => default
            };

            foreach (SyntaxToken modifier in modifiers)
            {
                string keyword = modifier.Text;

                if (AccessibilityRanks.Contains(keyword))
                {
                    return keyword;
                }
            }

            return GetDefaultAccessibility(member) switch
            {
                "public" => "public",
                "protected" => "protected",
                "internal" => "internal",
                _ => "private"
            };
        }

        private static string GetDefaultAccessibility(MemberDeclarationSyntax member)
        {
            if (member is ConstructorDeclarationSyntax constructor &&
                constructor.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                return "private";
            }

            if (member is DestructorDeclarationSyntax)
            {
                return "protected";
            }

            if (member.Parent is InterfaceDeclarationSyntax)
            {
                return "public";
            }

            return "private";
        }

        private static int GetAccessibilityRank(string keyword)
            => Array.IndexOf(AccessibilityRanks, keyword);

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
