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
    /// Naming checks over the handwritten sources.
    /// </summary>
    [TestFixture]
    public sealed class NamingConventionTests
    {
        private static readonly string[] BclTypeNames =
        [
            "Boolean", "Byte", "Char", "Decimal", "Double", "Int16", "Int32", "Int64",
            "Object", "SByte", "Single", "String", "UInt16", "UInt32", "UInt64"
        ];

        private static readonly string[] ForbiddenBooleanNames = ["flag", "check", "result"];

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingBuiltInTypes_ThenAliasesAreUsed(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                string[] bclTypeNames = [.. tree.GetRoot()
                    .DescendantNodes()
                    .OfType<IdentifierNameSyntax>()
                    .Where(IsTypePosition)
                    .Select(identifier => identifier.Identifier.ValueText)
                    .Where(name => BclTypeNames.Contains(name, StringComparer.Ordinal))
                    .Distinct(StringComparer.Ordinal)];

                Assert.That(
                    bclTypeNames,
                    Is.Empty,
                    $"{tree.FilePath}: Use built-in aliases instead of {string.Join(", ", bclTypeNames)}.");
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingTypeNames_ThenEachIsPascalCased(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                string[] misnamedTypes = [.. tree.GetRoot()
                    .DescendantNodes()
                    .OfType<BaseTypeDeclarationSyntax>()
                    .Select(declaration => declaration.Identifier.ValueText)
                    .Where(name => !IsPascalCase(name))];

                Assert.That(
                    misnamedTypes,
                    Is.Empty,
                    $"{tree.FilePath}: Every type must use PascalCase ({string.Join(", ", misnamedTypes)}).");
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingMethodNames_ThenEachIsPascalCased(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                string[] misnamedMethods = [.. tree.GetRoot()
                    .DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Select(method => method.Identifier.ValueText)
                    .Where(name => !IsPascalCase(name))];

                Assert.That(
                    misnamedMethods,
                    Is.Empty,
                    $"{tree.FilePath}: Every method must use PascalCase ({string.Join(", ", misnamedMethods)}).");
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingInterfaces_ThenEachUsesTheIPrefix(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                string[] misnamedInterfaces = [.. tree.GetRoot()
                    .DescendantNodes()
                    .OfType<InterfaceDeclarationSyntax>()
                    .Select(declaration => declaration.Identifier.ValueText)
                    .Where(IsMisnamedInterface)];

                Assert.That(
                    misnamedInterfaces,
                    Is.Empty,
                    $"{tree.FilePath}: Every interface must use the 'I' prefix followed by PascalCase.");
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingPrivateFields_ThenNoUnderscorePrefixIsUsed(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                string[] prefixedFields = [.. tree.GetRoot()
                    .DescendantNodes()
                    .OfType<FieldDeclarationSyntax>()
                    .Where(field => field.Modifiers.Any(SyntaxKind.PrivateKeyword))
                    .SelectMany(field => field.Declaration.Variables)
                    .Select(variable => variable.Identifier.ValueText)
                    .Where(name => name.StartsWith('_'))];

                Assert.That(
                    prefixedFields,
                    Is.Empty,
                    $"{tree.FilePath}: Private fields must be camelCase without an underscore prefix.");
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingBooleanNames_ThenPermissiblePrefixesAreUsed(string sourcePrefix)
        {
            foreach (SyntaxTree tree in CollectParseTrees(sourcePrefix))
            {
                string[] forbiddenNames = [.. tree.GetRoot()
                    .DescendantNodes()
                    .OfType<PropertyDeclarationSyntax>()
                    .Where(property => property.Type is PredefinedTypeSyntax predefined &&
                        predefined.Keyword.IsKind(SyntaxKind.BoolKeyword))
                    .Select(property => property.Identifier.ValueText)
                    .Where(name => ForbiddenBooleanNames.Contains(name.ToLowerInvariant(), StringComparer.Ordinal))];

                Assert.That(
                    forbiddenNames,
                    Is.Empty,
                    $"{tree.FilePath}: Boolean names must use Is/Has/Does/Are forms.");
            }
        }

        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        public void GivenTestSources_WhenCheckingFixtureNames_ThenEachEndsWithTheTestsSuffix(string sourcePrefix)
        {
            string[] fixtureNames = [.. CollectParseTrees(sourcePrefix)
                .SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
                .Where(declaration => declaration.AttributeLists
                    .SelectMany(list => list.Attributes)
                    .Any(attribute => attribute.Name.ToString().EndsWith("TestFixture", StringComparison.Ordinal)))
                .Select(declaration => declaration.Identifier.ValueText)];

            Assert.That(fixtureNames, Is.Not.Empty, "Vacuous-selector guard: no unit-test fixtures were found.");
            Assert.That(
                fixtureNames.Where(name => !name.EndsWith("Tests", StringComparison.Ordinal)),
                Is.Empty,
                "Violated convention: every test class name must end with the 'Tests' suffix.");
        }

        [Test]
        public void GivenUnitTestFixtures_WhenCheckingSubjects_ThenAnExportedProductionTypeExists()
        {
            string[] productionTypeNames = [.. ProductionArchitecture.ProductionAssembly
                .GetExportedTypes()
                .Select(type => type.Name)];

            IEnumerable<string> fixtureNames = CollectParseTrees(ArchitectureResources.UnitTestSourcePrefix)
                .SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
                .Select(declaration => declaration.Identifier.ValueText)
                .Select(name => name.Replace("Tests", string.Empty, StringComparison.Ordinal));

            foreach (string subjectName in fixtureNames)
            {
                Assert.That(
                    productionTypeNames,
                    Does.Contain(subjectName),
                    $"Violated convention: '{subjectName}' must be an exported production type.");
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

        private static bool IsTypePosition(IdentifierNameSyntax identifier)
            => identifier.Parent switch
            {
                VariableDeclarationSyntax declaration => declaration.Type == identifier,
                ParameterSyntax parameter => parameter.Type == identifier,
                MethodDeclarationSyntax method => method.ReturnType == identifier,
                PropertyDeclarationSyntax property => property.Type == identifier,
                FieldDeclarationSyntax field => field.Declaration.Type == identifier,
                CastExpressionSyntax cast => cast.Type == identifier,
                ObjectCreationExpressionSyntax creation => creation.Type == identifier,
                BaseTypeSyntax baseType => baseType.Type == identifier,
                TypeOfExpressionSyntax typeOf => typeOf.Type == identifier,
                DefaultExpressionSyntax defaultExpression => defaultExpression.Type == identifier,
                _ => false
            };

        private static bool IsPascalCase(string name)
            => !string.IsNullOrEmpty(name) && char.IsUpper(name[0]);

        private static bool IsMisnamedInterface(string name)
            => name.Length < 2 || name[0] != 'I' || !char.IsUpper(name[1]);
    }
}
