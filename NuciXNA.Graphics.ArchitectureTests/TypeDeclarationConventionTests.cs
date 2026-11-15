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
    /// Type declaration checks over the handwritten sources.
    /// </summary>
    [TestFixture]
    public sealed class TypeDeclarationConventionTests
    {
        /// <summary>
        /// The public effect classes are designed for host extension by inheritance and are documented as such in
        /// ARCHITECTURE.md, so they are the only narrowly scoped exemption from the sealing convention.
        /// </summary>
        private static readonly string[] InheritanceDesignExemptions =
        [
            "AnimationEffect",
            "FadeEffect",
            "MovementEffect",
            "OscillationEffect",
            "ZoomEffect"
        ];

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        public void GivenProductionSources_WhenCheckingSealing_ThenEachClassIsSealedAbstractOrStatic(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                ClassDeclarationSyntax[] unsealedClasses = [.. tree.GetRoot()
                    .DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .Where(declaration => !declaration.Modifiers.Any(modifier =>
                        modifier.IsKind(SyntaxKind.SealedKeyword) ||
                        modifier.IsKind(SyntaxKind.StaticKeyword) ||
                        modifier.IsKind(SyntaxKind.AbstractKeyword)))
                    .Where(declaration => !InheritanceDesignExemptions.Contains(
                        declaration.Identifier.ValueText,
                        StringComparer.Ordinal))];

                Assert.That(
                    unsealedClasses.Select(declaration => declaration.Identifier.ValueText),
                    Is.Empty,
                    $"{tree.FilePath}: Seal every class that is not designed for inheritance.");
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingEnumerations_ThenEveryVariantIsPascalCased(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                string[] misnamedVariants = [.. tree.GetRoot()
                    .DescendantNodes()
                    .OfType<EnumMemberDeclarationSyntax>()
                    .Select(member => member.Identifier.ValueText)
                    .Where(name => !char.IsUpper(name[0]))];

                Assert.That(
                    misnamedVariants,
                    Is.Empty,
                    $"{tree.FilePath}: Every enumeration variant must use PascalCase.");
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        public void GivenProductionSources_WhenCheckingInterfaces_ThenNonStaticClassesWithPublicMethodsHaveAnEquivalent(
            string sourcePrefix)
        {
            string[] interfaceNames = [.. CollectParseTrees(sourcePrefix)
                .SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<InterfaceDeclarationSyntax>())
                .Select(declaration => declaration.Identifier.ValueText)];
            List<string> violations = [];

            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                foreach (ClassDeclarationSyntax declaration in tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
                {
                    if (!NeedsEquivalentInterface(declaration))
                    {
                        continue;
                    }

                    string interfaceName = "I" + declaration.Identifier.ValueText;

                    if (!interfaceNames.Contains(interfaceName, StringComparer.Ordinal))
                    {
                        violations.Add(
                            $"{tree.FilePath}:{declaration.Identifier.ValueText} requires a colocated {interfaceName}.");
                    }
                }
            }

            Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        public void GivenProductionSources_WhenCheckingDocumentation_ThenEveryPublicNuGetMemberIsDocumented(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                List<string> violations = [];

                foreach (MemberDeclarationSyntax member in tree.GetRoot().DescendantNodes().OfType<MemberDeclarationSyntax>())
                {
                    if (!IsPublicNuGetMember(member) || IsDocumented(member))
                    {
                        continue;
                    }

                    violations.Add(
                        $"{tree.FilePath}:{member.GetLocation().GetLineSpan().StartLinePosition.Line + 1}: "
                            + "Add XML summary documentation to every public NuGet member.");
                }

                Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
            }
        }

        private static bool NeedsEquivalentInterface(ClassDeclarationSyntax declaration)
        {
            if (declaration.Modifiers.Any(SyntaxKind.StaticKeyword) ||
                declaration.Modifiers.Any(SyntaxKind.AbstractKeyword))
            {
                return false;
            }

            return declaration.Members
                .OfType<MethodDeclarationSyntax>()
                .Any(method => method.Modifiers.Any(SyntaxKind.PublicKeyword) &&
                    !method.Modifiers.Any(SyntaxKind.OverrideKeyword) &&
                    !method.Modifiers.Any(SyntaxKind.StaticKeyword));
        }

        private static bool IsPublicNuGetMember(MemberDeclarationSyntax member)
        {
            if (member is BaseTypeDeclarationSyntax typeDeclaration)
            {
                return typeDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword);
            }

            return member switch
            {
                MethodDeclarationSyntax method => method.Modifiers.Any(SyntaxKind.PublicKeyword),
                PropertyDeclarationSyntax property => property.Modifiers.Any(SyntaxKind.PublicKeyword),
                EventFieldDeclarationSyntax eventField => eventField.Modifiers.Any(SyntaxKind.PublicKeyword),
                FieldDeclarationSyntax field => field.Modifiers.Any(SyntaxKind.PublicKeyword),
                ConstructorDeclarationSyntax constructor => constructor.Modifiers.Any(SyntaxKind.PublicKeyword),
                EnumMemberDeclarationSyntax => true,
                _ => false
            };
        }

        private static bool IsDocumented(MemberDeclarationSyntax member)
        {
            if (member.GetLeadingTrivia().Any(trivia => trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)))
            {
                return true;
            }

            return member is EnumMemberDeclarationSyntax enumMember &&
                enumMember.Parent is EnumDeclarationSyntax enumDeclaration &&
                enumDeclaration.GetLeadingTrivia().Any(trivia => trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia));
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
