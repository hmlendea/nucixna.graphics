using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using ArchUnitNET.Domain;
using ArchUnitNET.Domain.Dependencies;
using ArchUnitNET.Fluent;
using ArchUnitNET.Fluent.Slices;

using NUnit.Framework;

using static ArchUnitNET.Fluent.ArchRuleDefinition;

using NuciXNA.Graphics.Drawing;
using NuciXNA.Graphics.SpriteEffects;

namespace NuciXNA.Graphics.ArchitectureTests
{
    /// <summary>
    /// Dependency and boundary checks over the loaded production architecture.
    /// </summary>
    [TestFixture]
    public sealed class DependencyRuleTests
    {
        private static readonly string[] ExpectedDrawingTypes =
        [
            "NuciXNA.Graphics.Drawing.Alignment",
            "NuciXNA.Graphics.Drawing.FontOutline",
            "NuciXNA.Graphics.Drawing.Sprite",
            "NuciXNA.Graphics.Drawing.StringDrawer",
            "NuciXNA.Graphics.Drawing.TextSprite",
            "NuciXNA.Graphics.Drawing.TextureDrawer",
            "NuciXNA.Graphics.Drawing.TextureLayout",
            "NuciXNA.Graphics.Drawing.TextureSprite"
        ];

        private static readonly string[] ExpectedEffectTypes =
        [
            "NuciXNA.Graphics.SpriteEffects.AnimationEffect",
            "NuciXNA.Graphics.SpriteEffects.FadeEffect",
            "NuciXNA.Graphics.SpriteEffects.MovementEffect",
            "NuciXNA.Graphics.SpriteEffects.NuciSpriteEffect`1",
            "NuciXNA.Graphics.SpriteEffects.OpacityEffect",
            "NuciXNA.Graphics.SpriteEffects.OscillationEffect",
            "NuciXNA.Graphics.SpriteEffects.RotationEffect",
            "NuciXNA.Graphics.SpriteEffects.ScaleEffect",
            "NuciXNA.Graphics.SpriteEffects.SpriteSheetEffect",
            "NuciXNA.Graphics.SpriteEffects.ZoomEffect"
        ];

        [Test]
        public void GivenProductionArchitecture_WhenLoaded_ThenOnlyTheProductionAssemblyIsModelled()
        {
            string[] modelledAssemblies = ProductionArchitecture.Model.Assemblies
                .Select(assembly => assembly.Name)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();

            Assert.That(
                modelledAssemblies,
                Is.EquivalentTo([ProductionArchitecture.ProductionAssembly.GetName().Name]),
                "Violated boundary: only the production assembly must be modelled.");
        }

        [Test]
        public void GivenProductionAssembly_WhenInspectingExportedTypes_ThenEveryDocumentedDrawingTypeIsPresent()
        {
            Assert.That(
                GetExportedTypeNames(),
                Is.SupersetOf(ExpectedDrawingTypes),
                "Violated boundary: a documented drawing type is missing from the public surface.");
        }

        [Test]
        public void GivenProductionAssembly_WhenInspectingExportedTypes_ThenEveryDocumentedEffectTypeIsPresent()
        {
            Assert.That(
                GetExportedTypeNames(),
                Is.SupersetOf(ExpectedEffectTypes),
                "Violated boundary: a documented sprite effect type is missing from the public surface.");
        }

        [Test]
        public void GivenProductionTypes_WhenCheckingNamespacePlacement_ThenAllResideInTheDocumentedNamespaces()
        {
            string[] unexpectedTypes = GetProductionTypeNames()
                .Where(name => !string.Equals(name, "NuciXNA.Graphics.GraphicsManager", StringComparison.Ordinal)
                    && !name.StartsWith("NuciXNA.Graphics.Drawing.", StringComparison.Ordinal)
                    && !name.StartsWith("NuciXNA.Graphics.SpriteEffects.", StringComparison.Ordinal))
                .ToArray();

            Assert.That(
                unexpectedTypes,
                Is.Empty,
                "Violated boundary: every production type must reside in NuciXNA.Graphics, "
                    + "NuciXNA.Graphics.Drawing, or NuciXNA.Graphics.SpriteEffects.");
        }

        [TestCase("NuciXNA.Graphics.Drawing.TextSprite")]
        [TestCase("NuciXNA.Graphics.Drawing.TextureDrawer")]
        [TestCase("NuciXNA.Graphics.Drawing.StringDrawer")]
        public void GivenEffects_WhenCheckingDependencies_ThenConcreteRenderersAreNotReferenced(string rendererTypeName)
        {
            IObjectProvider<IType> renderer = Types().That()
                .HaveFullName(rendererTypeName)
                .As($"the {rendererTypeName} renderer");

            IArchRule noRendererRule = Types().That()
                .ResideInNamespaceMatching(@"NuciXNA\.Graphics\.SpriteEffects(\.|$)")
                .Should()
                .NotDependOnAny(renderer)
                .Because("sprite effects mutate rendering parameters and must not know about concrete renderers");

            AssertHasTypes(Effects());
            AssertHasTypes(renderer);

            AssertNoViolations(noRendererRule);
        }

        [Test]
        public void GivenDrawingHelpers_WhenCheckingDependencies_ThenNoSpriteOrEffectTypeIsReferenced()
        {
            IObjectProvider<IType> drawingHelpers = Types().That()
                .ResideInNamespaceMatching(@"NuciXNA\.Graphics\.Drawing(\.|$)")
                .And().HaveNameEndingWith("Drawer")
                .As("stateless drawing helpers");

            IObjectProvider<IType> spritesAndEffects = Types().That()
                .ResideInNamespaceMatching(@"NuciXNA\.Graphics\.(Drawing|SpriteEffects)(\.|$)")
                .And().AreNotEnums()
                .And().DoNotHaveNameEndingWith("Drawer")
                .As("sprite and effect types");

            IArchRule statelessRule = Types().That()
                .Are(drawingHelpers)
                .Should()
                .NotDependOnAny(spritesAndEffects)
                .Because("TextureDrawer and StringDrawer are stateless utilities with no sprite coupling");

            AssertHasTypes(drawingHelpers);
            AssertHasTypes(spritesAndEffects);

            AssertNoViolations(statelessRule);
        }

        [Test]
        public void GivenGraphicsManager_WhenCheckingDependencies_ThenDrawingAndEffectNamespacesAreNotReferenced()
        {
            IObjectProvider<IType> graphicsManager = Types().That()
                .HaveFullName("NuciXNA.Graphics.GraphicsManager")
                .As("the graphics manager");

            IArchRule managerRule = Types().That()
                .Are(graphicsManager)
                .Should()
                .NotDependOnAnyTypesThat()
                .ResideInNamespaceMatching(@"NuciXNA\.Graphics\.Drawing(\.|$)")
                .AndShould()
                .NotDependOnAnyTypesThat()
                .ResideInNamespaceMatching(@"NuciXNA\.Graphics\.SpriteEffects(\.|$)")
                .Because("GraphicsManager only holds MonoGame device state");

            AssertHasTypes(graphicsManager);

            AssertNoViolations(managerRule);
        }

        [Test]
        public void GivenProductionTypes_WhenCheckingExternalDependencies_ThenNoPersistenceOrWebFrameworkIsUsed()
        {
            IArchRule frameworkRule = Types().That()
                .Are(ProductionArchitecture.ProductionTypes)
                .Should()
                .NotDependOnAnyTypesThat()
                .ResideInNamespaceMatching(@"Microsoft\.EntityFrameworkCore\..*")
                .AndShould()
                .NotDependOnAnyTypesThat()
                .ResideInNamespaceMatching(@"Microsoft\.AspNetCore\..*")
                .AndShould()
                .NotDependOnAnyTypesThat()
                .ResideInNamespaceMatching(@"Amazon\.DynamoDBv2\..*")
                .AndShould()
                .NotDependOnAnyTypesThat()
                .ResideInNamespaceMatching(@"Dapper\..*")
                .Because("the library is a rendering layer and must stay framework-agnostic");

            AssertHasTypes(ProductionArchitecture.ProductionTypes);

            AssertNoViolations(frameworkRule);
        }

        [Test]
        public void GivenProductionNamespaces_WhenCheckingDependencies_ThenTheDocumentedCompositionCycleIsTheOnlyCycle()
        {
            AssertHasTypes(ProductionArchitecture.ProductionTypes);

            string[][] cycles = [.. GetMutualNamespaceDependencies()
                .OrderBy(cycle => cycle[0], StringComparer.Ordinal)];

            Assert.That(
                cycles,
                Is.EqualTo(new[]
                {
                    new[] { "Drawing", "SpriteEffects" }
                }),
                "Violated boundary: Sprite composition makes Drawing and SpriteEffects mutually dependent; "
                    + "that single documented cycle is permitted and no other namespace may form one.");
        }

        [TestCase("NUnit")]
        [TestCase("Moq")]
        [TestCase("ArchUnit")]
        public void GivenProductionAssembly_WhenCheckingReferences_ThenTestFrameworksAreNotReferenced(string testAssembly)
        {
            string[] references = ProductionArchitecture.ProductionAssembly
                .GetReferencedAssemblies()
                .Select(reference => reference.Name)
                .OfType<string>()
                .ToArray();

            Assert.That(references, Is.Not.Empty, "Violated boundary: the assembly references must be inspectable.");
            Assert.That(
                references.Where(reference => reference.Contains(testAssembly, StringComparison.OrdinalIgnoreCase)),
                Is.Empty,
                $"Violated boundary: the production assembly must not reference {testAssembly}.");
        }

        [Test]
        public void GivenProductionAssembly_WhenCheckingFriendAssemblies_ThenInternalsAreNotExposed()
        {
            InternalsVisibleToAttribute[] friendAssemblies = ProductionArchitecture.ProductionAssembly
                .GetCustomAttributes<InternalsVisibleToAttribute>()
                .ToArray();

            Assert.That(
                friendAssemblies,
                Is.Empty,
                "Violated boundary: production code must not declare InternalsVisibleTo.");
        }

        [Test]
        public void GivenProductionAssembly_WhenCheckingDependencies_ThenOnlyDocumentedAssembliesAreReferenced()
        {
            string[] allowedPrefixes =
            [
                "System",
                "Microsoft.Xna.Framework",
                "MonoGame.Framework",
                "NuciXNA.DataAccess",
                "NuciXNA.Graphics",
                "NuciXNA.Primitives",
                "netstandard",
                "mscorlib"
            ];

            string[] undocumentedReferences = ProductionArchitecture.ProductionAssembly
                .GetReferencedAssemblies()
                .Select(reference => reference.Name)
                .OfType<string>()
                .Where(name => !allowedPrefixes.Any(prefix => name.StartsWith(prefix, StringComparison.Ordinal)))
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();

            Assert.That(
                undocumentedReferences,
                Is.Empty,
                "Violated boundary: production code must only reference the documented assemblies.");
        }

        [Test]
        public void GivenSpriteHierarchy_WhenInspectingDeclarations_ThenTheDocumentedInheritanceIsPreserved()
        {
            Assert.That(
                GetProductionType("NuciXNA.Graphics.Drawing.TextSprite").BaseType,
                Is.EqualTo(GetProductionType("NuciXNA.Graphics.Drawing.Sprite")),
                "Violated boundary: TextSprite must derive from Sprite.");

            Assert.That(
                GetProductionType("NuciXNA.Graphics.Drawing.TextureSprite").BaseType,
                Is.EqualTo(GetProductionType("NuciXNA.Graphics.Drawing.Sprite")),
                "Violated boundary: TextureSprite must derive from Sprite.");

            Type effectBase = GetProductionType("NuciXNA.Graphics.SpriteEffects.NuciSpriteEffect`1");

            foreach (string effectType in ExpectedEffectTypes.Where(name => !name.EndsWith("NuciSpriteEffect`1", StringComparison.Ordinal)))
            {
                Assert.That(
                    IsAssignableToOpenGeneric(GetProductionType(effectType), effectBase),
                    Is.True,
                    $"Violated boundary: {effectType} must derive from NuciSpriteEffect<TSprite>.");
            }
        }

        [Test]
        public void GivenDrawingHelpers_WhenInspectingDeclarations_ThenEachIsStatic()
        {
            foreach (string typeName in new[] { "NuciXNA.Graphics.Drawing.TextureDrawer", "NuciXNA.Graphics.Drawing.StringDrawer" })
            {
                Type helperType = GetProductionType(typeName);

                Assert.That(
                    helperType.IsAbstract && helperType.IsSealed,
                    Is.True,
                    $"Violated boundary: {typeName} must remain a static class.");
            }
        }

        [Test]
        public void GivenGraphicsManager_WhenInspectingDeclaration_ThenItIsASealedSingleton()
        {
            Type managerType = GetProductionType("NuciXNA.Graphics.GraphicsManager");

            Assert.That(managerType.IsSealed, Is.True, "Violated boundary: GraphicsManager must remain sealed.");

            ConstructorInfo[] publicConstructors = managerType
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            Assert.That(
                publicConstructors,
                Is.Empty,
                "Violated boundary: GraphicsManager must only be created through its static Instance.");

            Assert.That(
                managerType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static),
                Is.Not.Null,
                "Violated boundary: GraphicsManager must expose a static Instance property.");
        }

        [Test]
        public void GivenDisposableSpritesAndEffects_WhenInspectingDeclarations_ThenTheyImplementIDisposable()
        {
            string[] disposableTypes =
            [
                "NuciXNA.Graphics.Drawing.Sprite",
                "NuciXNA.Graphics.SpriteEffects.NuciSpriteEffect`1"
            ];

            foreach (string typeName in disposableTypes)
            {
                Assert.That(
                    GetProductionType(typeName).GetInterfaces(),
                    Does.Contain(typeof(IDisposable)),
                    $"Violated boundary: {typeName} must implement IDisposable.");
            }
        }

        [Test]
        public void GivenSpriteEffectBase_WhenInspectingDeclaration_ThenItIsGenericOverSprite()
        {
            Type effectBase = GetProductionType("NuciXNA.Graphics.SpriteEffects.NuciSpriteEffect`1");

            Assert.That(
                effectBase.IsGenericTypeDefinition,
                Is.True,
                "Violated boundary: NuciSpriteEffect must remain an open generic type.");

            Assert.That(
                effectBase.GetGenericArguments().Single().GetGenericParameterConstraints(),
                Does.Contain(GetProductionType("NuciXNA.Graphics.Drawing.Sprite")),
                "Violated boundary: NuciSpriteEffect<TSprite> must constrain TSprite to Sprite.");
        }

        [Test]
        public void GivenSpriteTypes_WhenInspectingConstructors_ThenParameterlessConstructionIsAvailable()
        {
            string[] constructibleTypes =
            [
                "NuciXNA.Graphics.Drawing.TextSprite",
                "NuciXNA.Graphics.Drawing.TextureSprite",
                "NuciXNA.Graphics.SpriteEffects.AnimationEffect",
                "NuciXNA.Graphics.SpriteEffects.FadeEffect",
                "NuciXNA.Graphics.SpriteEffects.MovementEffect",
                "NuciXNA.Graphics.SpriteEffects.OscillationEffect",
                "NuciXNA.Graphics.SpriteEffects.ZoomEffect"
            ];

            foreach (string typeName in constructibleTypes)
            {
                Assert.That(
                    GetProductionType(typeName).GetConstructor(Type.EmptyTypes),
                    Is.Not.Null,
                    $"Violated boundary: {typeName} must provide a parameterless constructor for host composition.");
            }
        }

        /// <summary>
        /// Returns each pair of production sub-namespaces that depend on one another, normalised into a stable order.
        /// </summary>
        private static IEnumerable<string[]> GetMutualNamespaceDependencies()
        {
            string[] namespaces = [.. ProductionArchitecture.Model.Types
                .Select(type => type.Namespace.FullName)
                .Where(name => name.StartsWith("NuciXNA.Graphics.", StringComparison.Ordinal))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(name => name, StringComparer.Ordinal)];

            HashSet<string> edges = [.. ProductionArchitecture.Model.Types
                .SelectMany(type => type.Dependencies.OfType<ITypeDependency>()
                    .Select(dependency => new
                    {
                        Source = type.Namespace.FullName,
                        Target = dependency.Target.Namespace.FullName
                    }))
                .Where(edge => !string.Equals(edge.Source, edge.Target, StringComparison.Ordinal))
                .Where(edge => edge.Source.StartsWith("NuciXNA.Graphics.", StringComparison.Ordinal) &&
                    edge.Target.StartsWith("NuciXNA.Graphics.", StringComparison.Ordinal))
                .Select(edge => $"{edge.Source}|{edge.Target}")];

            List<string[]> cycles = [];

            for (int firstIndex = 0; firstIndex < namespaces.Length; firstIndex += 1)
            {
                for (int secondIndex = firstIndex + 1; secondIndex < namespaces.Length; secondIndex += 1)
                {
                    string first = namespaces[firstIndex];
                    string second = namespaces[secondIndex];

                    if (edges.Contains($"{first}|{second}") && edges.Contains($"{second}|{first}"))
                    {
                        cycles.Add(
                        [
                            first.Substring("NuciXNA.Graphics.".Length),
                            second.Substring("NuciXNA.Graphics.".Length)
                        ]);
                    }
                }
            }

            return cycles;
        }

        private static IObjectProvider<IType> Effects()
            => Types().That()
                .ResideInNamespaceMatching(@"NuciXNA\.Graphics\.SpriteEffects(\.|$)")
                .As("sprite effects");

        private static void AssertHasTypes(IObjectProvider<IType> selection)
            => Assert.That(
                selection.GetObjects(ProductionArchitecture.Model),
                Is.Not.Empty,
                $"Vacuous-selector guard: the {selection.Description} selection matched no production types.");

        private static void AssertNoViolations(IArchRule archRule)
        {
            EvaluationResult[] failures = archRule
                .Evaluate(ProductionArchitecture.Model)
                .Where(result => !result.Passed)
                .ToArray();

            Assert.That(
                failures,
                Is.Empty,
                "Violated boundary: " + archRule.Description);
        }

        private static bool IsAssignableToOpenGeneric(Type candidate, Type openGenericBase)
        {
            Type? current = candidate;

            while (current is not null)
            {
                if (current.IsGenericType &&
                current.GetGenericTypeDefinition() == openGenericBase)
                {
                    return true;
                }

                current = current.BaseType;
            }

            return false;
        }

        private static string[] GetExportedTypeNames()
            => ProductionArchitecture.ProductionAssembly
                .GetExportedTypes()
                .Select(type => type.FullName)
                .OfType<string>()
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();

        private static string[] GetProductionTypeNames()
            => ProductionArchitecture.ProductionAssembly
                .GetTypes()
                .Where(type => !IsCompilerGenerated(type))
                .Select(type => type.FullName)
                .OfType<string>()
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();

        private static Type GetProductionType(string fullName)
        {
            Type? productionType = ProductionArchitecture.ProductionAssembly.GetType(fullName);

            Assert.That(productionType, Is.Not.Null, $"Violated boundary: {fullName} is missing.");

            return productionType!;
        }

        private static bool IsCompilerGenerated(Type type)
            => type.Name.StartsWith('<') || type.GetCustomAttribute<CompilerGeneratedAttribute>() is not null;
    }
}
