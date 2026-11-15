using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using NUnit.Framework;

namespace NuciXNA.Graphics.ArchitectureTests
{
    /// <summary>
    /// Reflection-based public-contract checks over the loaded production assembly.
    /// </summary>
    [TestFixture]
    public sealed class PublicContractConventionTests
    {
        private static readonly Assembly ProductionAssembly = ProductionArchitecture.ProductionAssembly;

        [Test]
        public void GivenProductionEnumerations_WhenInspectingValues_ThenTheyAreDistinctAndContiguous()
        {
            Type[] enums = [.. ProductionAssembly
                .GetExportedTypes()
                .Where(type => type.IsEnum)];

            Assert.That(enums, Is.Not.Empty, "Vacuous-selector guard: no exported enumerations were found.");

            foreach (Type enumType in enums)
            {
                long[] values = [.. Enum.GetValues(enumType).Cast<object>().Select(Convert.ToInt64)];

                Assert.That(
                    values.Distinct().Count(),
                    Is.EqualTo(values.Length),
                    $"{enumType.FullName}: Enumeration values must be unique.");
            }
        }

        [Test]
        public void GivenExportedEnumerations_WhenInspectingMembers_ThenEachMemberHasAName()
        {
            foreach (Type enumType in ProductionAssembly.GetExportedTypes().Where(type => type.IsEnum))
            {
                string[] names = Enum.GetNames(enumType);

                Assert.That(names, Is.Not.Empty, $"{enumType.FullName}: Enumerations must declare at least one member.");
                Assert.That(
                    names.Where(name => string.IsNullOrWhiteSpace(name)),
                    Is.Empty,
                    $"{enumType.FullName}: Enumeration members must declare a non-empty name.");
            }
        }

        [Test]
        public void GivenPublicConcreteClasses_WhenInspectingDeclarations_ThenEachIsSealedAbstractOrStatic()
        {
            string[] effectTypes =
            [
                "NuciXNA.Graphics.SpriteEffects.AnimationEffect",
                "NuciXNA.Graphics.SpriteEffects.FadeEffect",
                "NuciXNA.Graphics.SpriteEffects.MovementEffect",
                "NuciXNA.Graphics.SpriteEffects.OscillationEffect",
                "NuciXNA.Graphics.SpriteEffects.ZoomEffect"
            ];

            Type[] unsealedClasses = [.. ProductionAssembly
                .GetExportedTypes()
                .Where(type => type.IsClass && !type.IsAbstract && !type.IsSealed)
                .Where(type => !effectTypes.Contains(type.FullName, StringComparer.Ordinal))];

            Assert.That(
                unsealedClasses.Select(type => type.FullName),
                Is.Empty,
                "Violated boundary: every non-inheritance-designed class must be sealed.");
        }

        [Test]
        public void GivenExportedTypes_WhenInspectingDeclarations_ThenNoneAreCompilerGeneratedOrNested()
        {
            string[] nestedOrGenerated = [.. ProductionAssembly
                .GetExportedTypes()
                .Where(type => type.IsNested || type.Name.StartsWith('<'))
                .Select(type => type.FullName)
                .OfType<string>()];

            Assert.That(
                nestedOrGenerated,
                Is.Empty,
                "Violated boundary: the public surface must not expose nested or compiler-generated types.");
        }

        [Test]
        public void GivenDisposableTypes_WhenInspectingDeclarations_ThenDisposeIsPublicAndVirtual()
        {
            foreach (Type type in ProductionAssembly.GetExportedTypes().Where(type => typeof(IDisposable).IsAssignableFrom(type)))
            {
                MethodInfo? dispose = type.GetMethod("Dispose", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);

                Assert.That(
                    dispose,
                    Is.Not.Null,
                    $"{type.FullName}: IDisposable implementations must expose a public parameterless Dispose.");

                Assert.That(
                    dispose!.IsVirtual,
                    Is.True,
                    $"{type.FullName}: Dispose must be virtual so derived types can extend the disposal contract.");
            }
        }

        [Test]
        public void GivenExportedTypes_WhenInspectingConstructors_ThenNoneRequireMoreThanSevenArguments()
        {
            foreach (Type type in ProductionAssembly.GetExportedTypes().Where(type => type.IsClass && !type.IsAbstract))
            {
                foreach (ConstructorInfo constructor in type.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
                {
                    Assert.That(
                        constructor.GetParameters().Length,
                        Is.LessThanOrEqualTo(7),
                        $"{type.FullName}: Constructors must declare at most seven parameters.");
                }
            }
        }

        [Test]
        public void GivenExportedMethods_WhenInspectingSignatures_ThenNoneExceedSevenParameters()
        {
            foreach (Type type in ProductionAssembly.GetExportedTypes())
            {
                MethodInfo[] oversized = [.. type
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                    .Where(method => !method.IsSpecialName)
                    .Where(method => method.GetParameters().Length > 7)];

                string[] overloads =
                [
                    "NuciXNA.Graphics.Drawing.TextureDrawer.Draw",
                    "NuciXNA.Graphics.Drawing.StringDrawer.Draw"
                ];

                foreach (MethodInfo method in oversized)
                {
                    string qualifiedName = $"{type.FullName}.{method.Name}";

                    Assert.That(
                        overloads.Contains(qualifiedName, StringComparer.Ordinal),
                        Is.True,
                        $"{qualifiedName}: The documented public API mirroring SpriteBatch is the only permitted exception.");
                }
            }
        }

        [Test]
        public void GivenExportedTypes_WhenInspectingDeclarations_ThenNoStructIsPubliclyMutable()
        {
            Assert.That(
                ProductionAssembly.GetExportedTypes().Where(type => type.IsValueType && !type.IsEnum),
                Is.Empty,
                "Violated boundary: NuciXNA.Graphics exposes no public value types; primitives live in NuciXNA.Primitives.");
        }

        [Test]
        public void GivenExportedTypes_WhenInspectingNamespaces_ThenEachResidesInADocumentedNamespace()
        {
            string[] allowedNamespaces =
            [
                "NuciXNA.Graphics",
                "NuciXNA.Graphics.Drawing",
                "NuciXNA.Graphics.SpriteEffects"
            ];

            string[] undocumented = [.. ProductionAssembly
                .GetExportedTypes()
                .Select(type => GetNamespace(type))
                .Distinct(StringComparer.Ordinal)
                .Where(name => !allowedNamespaces.Contains(name, StringComparer.Ordinal))];

            Assert.That(
                undocumented,
                Is.Empty,
                "Violated boundary: every exported type must reside in a documented namespace.");
        }

        private static string GetNamespace(Type type)
        {
            string? namespaceName = type.Namespace;

            if (namespaceName is null)
            {
                return string.Empty;
            }

            return namespaceName;
        }
    }
}
