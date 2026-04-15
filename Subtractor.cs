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

internal class SubtractorBuilding
{
    private readonly BuildingDefinitionId _defId = new("raphdf201-subtractor");
    private readonly ILogger _logger;

    internal SubtractorBuilding(ILogger logger)
    {
        _logger = logger;

        IBuildingGroupBuilder bldingGroup = BuildingGroup.Create(new BuildingDefinitionGroupId("raphdf201-subtractor-group"))
            .WithTitle("building-variant.raphdf201-subtractor.title".T())
            .WithDescription("building-variant.raphdf201-subtractor.description".T())
            .WithIcon(FileTextureLoader.LoadTextureAsSprite(Main.Res.SubPath("subtractor.png"), out _))
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
            .DynamicallyRendering<SubtractorSimulationRenderer, SubtractorSimulation, ISubtractorDrawData>(
                new SubtractorDrawData())
            .WithStaticDrawData(SubtractorDrawData.CreateDrawData())
            .WithoutSound()
            .WithoutSimulationConfiguration()
            .WithEfficiencyData(new BuildingEfficiencyData(2, 1));

        AtomicBuildings.Extend()
            .AllScenarios()
            .WithBuilding(blding, bldingGroup)
            .UnlockedAtMilestone(new ByIndexMilestoneSelector(2))
            .WithDefaultPlacement()
            .InToolbar(ToolbarElementLocator.Root().ChildAt(2).ChildAt(6).ChildAt(^1).InsertAfter())
            .WithSimulation(new SubtractorFactoryBuilder(), _logger)
            .WithCustomModules(new SubtractorModuleDataProvider())
            .WithoutPrediction()
            .Build();
    }

    internal AtomicStatefulBuildingSimulationSystem<SubtractorSimulation, LogicGate2In1OutSimulationState> Register()
    {
        return new AtomicStatefulBuildingSimulationSystem<SubtractorSimulation, LogicGate2In1OutSimulationState>(
            new SubtractorSimulationFactory(), _defId, _logger);
    }
}

internal class SubtractorDrawData : ISubtractorDrawData
{
    internal static BuildingDrawData CreateDrawData()
    {
        var baseMeshPath = Main.Res.SubPath("subtractor.fbx");
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
            new SubtractorDrawData(),
            false,
            null,
            false);
    }
}

internal class SubtractorFactoryBuilder
    : IBuildingSimulationFactoryBuilder<SubtractorSimulation, LogicGate2In1OutSimulationState,
        EmptyCustomSimulationConfiguration>
{
    public IFactory<LogicGate2In1OutSimulationState, SubtractorSimulation> BuildFactory(
        SimulationSystemsDependencies dependencies,
        [UnscopedRef] out EmptyCustomSimulationConfiguration config)
    {
        config = new EmptyCustomSimulationConfiguration();
        return new SubtractorSimulationFactory();
    }
}

internal class SubtractorModuleDataProvider :
    SimulationBasedBuildingModuleDataProvider<SubtractorSimulation>
{
    protected override IEnumerable<IHUDSidePanelModuleData> GetSimulationModules(
        BuildingModel building,
        ILocalizedSimulation localizedSimulation,
        SubtractorSimulation actualSimulation)
    {
        yield return new HUDSidePanelModuleWireInfo.Data(
            "Input 1", actualSimulation.Input0Conductor);
        yield return new HUDSidePanelModuleWireInfo.Data(
            "Input 2", actualSimulation.Input1Conductor);
        yield return new HUDSidePanelModuleWireInfo.Data(
            "Output", actualSimulation.OutputConductor);
    }
}

internal class SubtractorSimulation(LogicGate2In1OutSimulationState state)
    : LogicGate2In1OutSimulation(state)
{
    protected override ISignal ComputeOutputSignal(ISignal a, ISignal b)
    {
        if (a is IntegerSignal sig1 && b is IntegerSignal sig2)
        {
            return IntegerSignal.Get(sig1.Value - sig2.Value);
        }

        return NullSignal.Instance;
    }
}

internal class SubtractorSimulationFactory :
    IFactory<LogicGate2In1OutSimulationState, SubtractorSimulation>
{
    public SubtractorSimulation Produce(LogicGate2In1OutSimulationState state)
    {
        return new SubtractorSimulation(state);
    }
}

internal class SubtractorSimulationRenderer(IMapModel map)
    : StatelessBuildingSimulationRenderer<SubtractorSimulation, ISubtractorDrawData>(map);

internal interface ISubtractorDrawData : IBuildingCustomDrawData;
