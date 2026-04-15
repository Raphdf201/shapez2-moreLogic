using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Core.Factory;
using Core.Localization;
using Game.Content.Features.Signals;
using Game.Core.Map.Simulation;
using ShapezShifter.Flow;
using ShapezShifter.Flow.Atomic;
using ShapezShifter.Flow.Research;
using ShapezShifter.Flow.Toolbar;
using ShapezShifter.Hijack;
using ShapezShifter.Kit;
using ShapezShifter.Textures;
using UnityEngine;
using ILogger = Core.Logging.ILogger;

namespace MoreLogic;

internal class NAndBuilding
{
    private readonly BuildingDefinitionId _defId = new("raphdf201-nand");
    private readonly ILogger _logger;

    internal NAndBuilding(ILogger logger)
    {
        _logger = logger;

        IBuildingGroupBuilder bldingGroup = BuildingGroup.Create(new BuildingDefinitionGroupId("raphdf201-nand-group"))
            .WithTitle("building-variant.raphdf201-nand.title".T())
            .WithDescription("building-variant.raphdf201-nand.description".T())
            .WithIcon(FileTextureLoader.LoadTextureAsSprite(Main.Res.SubPath("nand.png"), out _))
            .AsNonTransportableBuilding()
            .WithPreferredPlacement(DefaultPreferredPlacementMode.Single)
            .AutoConnected();

        IBuildingConnectorData connectorData = BuildingConnectors.SingleTile()
            .AddWireInput(WireConnectorConfig.CustomInput(TileDirection.West))
            .AddWireInput(WireConnectorConfig.CustomInput(TileDirection.East))
            .AddWireOutput(WireConnectorConfig.CustomOutput(TileDirection.North))
            .Build();

        IBuildingBuilder blding = Building.Create(_defId)
            .WithConnectorData(connectorData)
            .DynamicallyRendering<NAndGateSimulationRenderer, NAndGateSimulation, INAndGateDrawData>(
                new NAndGateDrawData())
            .WithStaticDrawData(NAndGateDrawData.CreateDrawData())
            .WithoutSound()
            .WithoutSimulationConfiguration()
            .WithEfficiencyData(new BuildingEfficiencyData(2, 1));

        AtomicBuildings.Extend()
            .AllScenarios()
            .WithBuilding(blding, bldingGroup)
            .UnlockedAtMilestone(new ByIndexMilestoneSelector(2))
            .WithDefaultPlacement()
            .InToolbar(ToolbarElementLocator.Root().ChildAt(2).ChildAt(6).ChildAt(^1).InsertAfter())
            .WithSimulation(new NAndGateFactoryBuilder(), _logger)
            .WithCustomModules(new NAndGateModuleDataProvider())
            .WithoutPrediction()
            .Build();
    }

    internal AtomicStatefulBuildingSimulationSystem<NAndGateSimulation, LogicGate2In1OutSimulationState> Register()
    {
        return new AtomicStatefulBuildingSimulationSystem<NAndGateSimulation, LogicGate2In1OutSimulationState>(
            new NAndGateSimulationFactory(), _defId, _logger);
    }
}

internal class NAndGateDrawData : INAndGateDrawData
{
    internal static BuildingDrawData CreateDrawData()
    {
        var baseMeshPath = Main.Res.SubPath("nand.fbx");
        Mesh baseMesh = FileMeshLoader.LoadSingleMeshFromFile(baseMeshPath);
        LOD6Mesh lodMesh = MeshLod.Create().AddLod0Mesh(baseMesh)
            .UseLod0AsLod1()
            .UseLod1AsLod2()
            .UseLod2AsLod3()
            .UseLod3AsLod4()
            .UseLod4AsLod5()
            .BuildLod6Mesh();

        return new BuildingDrawData(
            false,
            [lodMesh, lodMesh, lodMesh],
            lodMesh,
            lodMesh,
            lodMesh.LODClose,
            new LODEmptyMesh(),
            BoundingBoxHelper.CreateBasicCollider(baseMesh),
            new NAndGateDrawData(),
            false,
            null,
            false);
    }
}

internal class NAndGateFactoryBuilder
    : IBuildingSimulationFactoryBuilder<NAndGateSimulation, LogicGate2In1OutSimulationState,
        EmptyCustomSimulationConfiguration>
{
    public IFactory<LogicGate2In1OutSimulationState, NAndGateSimulation> BuildFactory(
        SimulationSystemsDependencies dependencies,
        [UnscopedRef] out EmptyCustomSimulationConfiguration config)
    {
        config = new EmptyCustomSimulationConfiguration();
        return new NAndGateSimulationFactory();
    }
}

internal class NAndGateModuleDataProvider :
    SimulationBasedBuildingModuleDataProvider<NAndGateSimulation>
{
    protected override IEnumerable<IHUDSidePanelModuleData> GetSimulationModules(
        BuildingModel building,
        ILocalizedSimulation localizedSimulation,
        NAndGateSimulation actualSimulation)
    {
        yield return new HUDSidePanelModuleWireInfo.Data(
            "Input 1", actualSimulation.Input0Conductor);
        yield return new HUDSidePanelModuleWireInfo.Data(
            "Input 2", actualSimulation.Input1Conductor);
        yield return new HUDSidePanelModuleWireInfo.Data(
            "Output", actualSimulation.OutputConductor);
    }
}

internal class NAndGateSimulation(LogicGate2In1OutSimulationState state) : LogicGate2In1OutSimulation(state)
{
    protected override ISignal ComputeOutputSignal(ISignal a, ISignal b)
    {
        return IntegerSignal.Get(!(a.IsTruthy() && b.IsTruthy()));
    }
}

internal class NAndGateSimulationFactory :
    IFactory<LogicGate2In1OutSimulationState, NAndGateSimulation>
{
    public NAndGateSimulation Produce(LogicGate2In1OutSimulationState state)
    {
        return new NAndGateSimulation(state);
    }
}

internal class NAndGateSimulationRenderer(IMapModel map)
    : StatelessBuildingSimulationRenderer<NAndGateSimulation, INAndGateDrawData>(map);

internal interface INAndGateDrawData : IBuildingCustomDrawData;
