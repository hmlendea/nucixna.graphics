using ArchUnitNET.Domain;
using ArchUnitNET.Loader;

using static ArchUnitNET.Fluent.ArchRuleDefinition;

using Assembly = System.Reflection.Assembly;

using NuciXNA.Graphics.Drawing;

namespace NuciXNA.Graphics.ArchitectureTests
{
    /// <summary>
    /// Holds the loaded production architecture shared by every dependency rule.
    /// </summary>
    internal static class ProductionArchitecture
    {
        internal static Assembly ProductionAssembly { get; } = typeof(TextSprite).Assembly;

        internal static Architecture Model { get; } = new ArchLoader()
            .LoadAssemblies(ProductionAssembly)
            .Build();

        internal static IObjectProvider<IType> ProductionTypes { get; } = Types()
            .That()
            .ResideInAssembly(ProductionAssembly)
            .As("NuciXNA.Graphics production types");
    }
}
