using System.Collections.Generic;
using System.Linq;
using Core.Logging;
using Game.Orchestration;
using MonoMod.RuntimeDetour;
using ShapezShifter.Kit;
using ShapezShifter.SharpDetour;

namespace MoreLogic;

public class Main : IMod
{
    internal static readonly ModFolderLocator Res = ModDirectoryLocator.CreateLocator<Main>().SubLocator("Resources");
    private readonly Hook _modSystemHook;
    private readonly AdderBuilding _adder;
    private readonly MultiplierBuilding _multiplier;
    private readonly NAndBuilding _nand;

    public Main(ILogger logger)
    {
        _adder = new AdderBuilding(logger);
        _multiplier = new MultiplierBuilding(logger);
        _nand = new NAndBuilding(logger);

        _modSystemHook = DetourHelper
            .CreatePostfixHook<BuiltinSimulationSystems, IEnumerable<ISimulationSystem>>(
                simulationSystems => simulationSystems.CreateSimulationSystems(),
                CreateModSystems);
    }

    public void Dispose()
    {
        _modSystemHook.Dispose();
    }

    private IEnumerable<ISimulationSystem> CreateModSystems(
        BuiltinSimulationSystems builtinSimulationSystems,
        IEnumerable<ISimulationSystem> systems)
    {
        return systems
            .Append(_adder.Register())
            .Append(_multiplier.Register())
            .Append(_nand.Register());
    }
}
