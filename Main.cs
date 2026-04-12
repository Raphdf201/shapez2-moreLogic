using System.Collections.Generic;
using System.Linq;
using Core.Localization;
using Game.Orchestration;
using MonoMod.RuntimeDetour;
using ShapezShifter.Flow;
using ShapezShifter.Flow.Atomic;
using ShapezShifter.Flow.Research;
using ShapezShifter.Flow.Toolbar;
using ShapezShifter.Kit;
using ShapezShifter.SharpDetour;
using ShapezShifter.Textures;

namespace MoreLogic;

public class Main : IMod
{
    private readonly Hook _modSystemHook;
    private readonly BuildingDefinitionId _defId = new("nand");
    private readonly IToolbarEntryInsertLocation _location =
        ToolbarElementLocator.Root().ChildAt(2).ChildAt(6).ChildAt(^1).InsertAfter();
    internal static readonly ModFolderLocator Res = ModDirectoryLocator.CreateLocator<Main>().SubLocator("Resources");

    public Main()
    {
        IBuildingGroupBuilder bldingGroup = BuildingGroup.Create(new("nandgroup"))
            .WithTitle("building-variant.nand-gate.title".T())
            .WithDescription("building-variant.nand-gate.description".T())
            .WithIcon(FileTextureLoader.LoadTextureAsSprite(Res.SubPath("icon.png"), out _))
            .AsNonTransportableBuilding()
            .WithPreferredPlacement(DefaultPreferredPlacementMode.Single)
            .AutoConnected();

        IBuildingConnectorData connectorData = BuildingConnectors.SingleTile()
            .AddWireInput(WireConnectorConfig.CustomInput(TileDirection.South))
            .AddWireInput(WireConnectorConfig.CustomInput(TileDirection.North))
            .AddWireOutput(WireConnectorConfig.CustomOutput(TileDirection.East))
            .Build();

        IBuildingBuilder blding = Building.Create(_defId)
            .WithConnectorData(connectorData)
            .DynamicallyRendering<NAndGateSimulationRenderer, NAndGateSimulation, INAndGateDrawData>(new NAndGateDrawData())
            .WithStaticDrawData(NAndGateDrawData.CreateDrawData())
            .WithoutPrediction()
            .WithoutSound()
            .WithoutSimulationConfiguration()
            .WithEfficiencyData(new BuildingEfficiencyData(2, 1));

        AtomicBuildings.Extend()
            .AllScenarios()
            .WithBuilding(blding, bldingGroup)
            .UnlockedAtMilestone(new ByIndexMilestoneSelector(2))
            .WithDefaultPlacement()
            .InToolbar(_location)
            .WithSimulation(new NAndGateFactoryBuilder())
            .WithCustomModules(new NAndGateModuleDataProvider())
            .Build();

        _modSystemHook = DetourHelper
            .CreatePostfixHook<BuiltinSimulationSystems, IEnumerable<ISimulationSystem>>(
                simulationSystems => simulationSystems.CreateSimulationSystems(),
                CreateModSystems);

        this.RegisterConsoleCommand("version", context => context.Output("v0.0.1"));
    }

    public void Dispose()
    {
        _modSystemHook.Dispose();
    }

    private IEnumerable<ISimulationSystem> CreateModSystems(
        BuiltinSimulationSystems builtinSimulationSystems,
        IEnumerable<ISimulationSystem> systems)
    {
        return systems.Append(new AtomicStatefulBuildingSimulationSystem<NAndGateSimulation, LogicGate2In1OutSimulationState>(
            new NAndGateSimulationFactory(), _defId));
    }
}
