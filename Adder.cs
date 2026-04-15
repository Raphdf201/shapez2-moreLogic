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

internal class AdderBuilding
{
    private readonly BuildingDefinitionId _defId = new("raphdf201-adder");
    private readonly ILogger _logger;

    internal AdderBuilding(ILogger logger)
    {
        _logger = logger;

        IBuildingGroupBuilder bldingGroup = BuildingGroup.Create(new BuildingDefinitionGroupId("raphdf201-adder-group"))
            .WithTitle("building-variant.raphdf201-adder.title".T())
            .WithDescription("building-variant.raphdf201-adder.description".T())
            .WithIcon(FileTextureLoader.LoadTextureAsSprite(Main.Res.SubPath("adder.png"), out _))
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
            .DynamicallyRendering<AdderSimulationRenderer, AdderSimulation, IAdderDrawData>(
                new AdderDrawData())
            .WithStaticDrawData(AdderDrawData.CreateDrawData())
            .WithoutSound()
            .WithoutSimulationConfiguration()
            .WithEfficiencyData(new BuildingEfficiencyData(2, 1));

        AtomicBuildings.Extend()
            .AllScenarios()
            .WithBuilding(blding, bldingGroup)
            .UnlockedAtMilestone(new ByIndexMilestoneSelector(2))
            .WithDefaultPlacement()
            .InToolbar(ToolbarElementLocator.Root().ChildAt(2).ChildAt(6).ChildAt(^1).InsertAfter())
            .WithSimulation(new AdderFactoryBuilder(), _logger)
            .WithCustomModules(new AdderModuleDataProvider())
            .WithoutPrediction()
            .Build();
    }

    internal AtomicStatefulBuildingSimulationSystem<AdderSimulation, LogicGate2In1OutSimulationState> Register()
    {
        return new AtomicStatefulBuildingSimulationSystem<AdderSimulation, LogicGate2In1OutSimulationState>(
            new AdderSimulationFactory(), _defId, _logger);
    }
}

internal class AdderDrawData : IAdderDrawData
{
    internal static BuildingDrawData CreateDrawData()
    {
        var baseMeshPath = Main.Res.SubPath("adder.fbx");
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
            new AdderDrawData(),
            false,
            null,
            false);
    }
}

internal class AdderFactoryBuilder
    : IBuildingSimulationFactoryBuilder<AdderSimulation, LogicGate2In1OutSimulationState,
        EmptyCustomSimulationConfiguration>
{
    public IFactory<LogicGate2In1OutSimulationState, AdderSimulation> BuildFactory(
        SimulationSystemsDependencies dependencies,
        [UnscopedRef] out EmptyCustomSimulationConfiguration config)
    {
        config = new EmptyCustomSimulationConfiguration();
        return new AdderSimulationFactory();
    }
}

internal class AdderModuleDataProvider :
    SimulationBasedBuildingModuleDataProvider<AdderSimulation>
{
    protected override IEnumerable<IHUDSidePanelModuleData> GetSimulationModules(
        BuildingModel building,
        ILocalizedSimulation localizedSimulation,
        AdderSimulation actualSimulation)
    {
        yield return new HUDSidePanelModuleWireInfo.Data(
            "Input 1", actualSimulation.Input0Conductor);
        yield return new HUDSidePanelModuleWireInfo.Data(
            "Input 2", actualSimulation.Input1Conductor);
        yield return new HUDSidePanelModuleWireInfo.Data(
            "Output", actualSimulation.OutputConductor);
    }
}

internal class AdderSimulation(LogicGate2In1OutSimulationState state)
    : LogicGate2In1OutSimulation(state)
{
    protected override ISignal ComputeOutputSignal(ISignal a, ISignal b)
    {
        if (a is IntegerSignal sig1 && b is IntegerSignal sig2)
        {
            return IntegerSignal.Get(sig1.Value + sig2.Value);
        }

        return NullSignal.Instance;
    }
}

internal class AdderSimulationFactory :
    IFactory<LogicGate2In1OutSimulationState, AdderSimulation>
{
    public AdderSimulation Produce(LogicGate2In1OutSimulationState state)
    {
        return new AdderSimulation(state);
    }
}

internal class AdderSimulationRenderer(IMapModel map)
    : StatelessBuildingSimulationRenderer<AdderSimulation, IAdderDrawData>(map);

internal interface IAdderDrawData : IBuildingCustomDrawData;
