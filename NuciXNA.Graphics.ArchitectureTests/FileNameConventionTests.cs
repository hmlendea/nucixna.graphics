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
    /// Declaration-placement checks over the handwritten sources.
    /// </summary>
    [TestFixture]
    public sealed class FileNameConventionTests
    {
        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingDeclarations_ThenEachFileDeclaresExactlyOneType(string sourcePrefix)
        {
            foreach (string source in CollectSources(sourcePrefix))
            {
                SyntaxTree tree = ParseTree(sourcePrefix, source);
                string[] declaredTypes = GetDeclaredTypeNames(tree).ToArray();

                Assert.That(
                    declaredTypes,
                    Has.Length.EqualTo(1),
                    $"Violated convention in {source}: declare exactly one type per file, including nested types.");
            }
        }

        [TestCase(ArchitectureResources.ProductionSourcePrefix)]
        [TestCase(ArchitectureResources.UnitTestSourcePrefix)]
        [TestCase(ArchitectureResources.ArchitectureTestSourcePrefix)]
        public void GivenSources_WhenCheckingTypeNames_ThenEachMatchesItsFileNameCaseSensitively(string sourcePrefix)
        {
            foreach (string source in CollectSources(sourcePrefix))
            {
                SyntaxTree tree = ParseTree(sourcePrefix, source);
                string fileNameStem = source.Substring(source.LastIndexOf('/') + 1);
                fileNameStem = fileNameStem.Substring(0, fileNameStem.LastIndexOf('.'));

                foreach (string typeName in GetDeclaredTypeNames(tree))
                {
                    Assert.That(
                        RemoveGenericArity(typeName),
                        Is.EqualTo(fileNameStem),
                        $"Violated convention in {source}: type '{typeName}' must match the file name stem '{fileNameStem}'.");
                }
            }
        }

        private static SyntaxTree ParseTree(string sourcePrefix, string source)
            => CSharpSyntaxTree.ParseText(ArchitectureResources.ReadResource(sourcePrefix, source), path: source);

        private static IEnumerable<string> CollectSources(string sourcePrefix)
        {
            IReadOnlyList<string> sources = ArchitectureResources.GetResourcePaths(sourcePrefix);

            Assert.That(
                sources,
                Is.Not.Empty,
                $"Violated convention: no source files were found under '{sourcePrefix}'.");

            return sources;
        }

        private static IEnumerable<string> GetDeclaredTypeNames(SyntaxTree tree)
            => tree.GetRoot()
                .DescendantNodes()
                .OfType<BaseTypeDeclarationSyntax>()
                .Select(declaration => declaration.Identifier.ValueText)
                .Concat(tree.GetRoot()
                    .DescendantNodes()
                    .OfType<DelegateDeclarationSyntax>()
                    .Select(declaration => declaration.Identifier.ValueText));

        private static string RemoveGenericArity(string typeName)
        {
            int arityIndex = typeName.IndexOf('`');

            if (arityIndex < 0)
            {
                return typeName;
            }

            return typeName.Substring(0, arityIndex);
        }
    }
}
