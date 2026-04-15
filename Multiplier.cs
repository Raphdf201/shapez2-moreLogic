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

internal class MultiplierBuilding
{
    private readonly BuildingDefinitionId _defId = new("raphdf201-multiplier");
    private readonly ILogger _logger;

    internal MultiplierBuilding(ILogger logger)
    {
        _logger = logger;

        IBuildingGroupBuilder bldingGroup = BuildingGroup.Create(new BuildingDefinitionGroupId("raphdf201-multiplier-group"))
            .WithTitle("building-variant.raphdf201-multiplier.title".T())
            .WithDescription("building-variant.raphdf201-multiplier.description".T())
            .WithIcon(FileTextureLoader.LoadTextureAsSprite(Main.Res.SubPath("multiplier.png"), out _))
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
            .DynamicallyRendering<MultiplierSimulationRenderer, MultiplierSimulation, IMultiplierDrawData>(
                new MultiplierDrawData())
            .WithStaticDrawData(MultiplierDrawData.CreateDrawData())
            .WithoutSound()
            .WithoutSimulationConfiguration()
            .WithEfficiencyData(new BuildingEfficiencyData(2, 1));

        AtomicBuildings.Extend()
            .AllScenarios()
            .WithBuilding(blding, bldingGroup)
            .UnlockedAtMilestone(new ByIndexMilestoneSelector(2))
            .WithDefaultPlacement()
            .InToolbar(ToolbarElementLocator.Root().ChildAt(2).ChildAt(6).ChildAt(^1).InsertAfter())
            .WithSimulation(new MultiplierFactoryBuilder(), _logger)
            .WithCustomModules(new MultiplierModuleDataProvider())
            .WithoutPrediction()
            .Build();
    }

    internal AtomicStatefulBuildingSimulationSystem<MultiplierSimulation, LogicGate2In1OutSimulationState> Register()
    {
        return new AtomicStatefulBuildingSimulationSystem<MultiplierSimulation, LogicGate2In1OutSimulationState>(
            new MultiplierSimulationFactory(), _defId, _logger);
    }
}

internal class MultiplierDrawData : IMultiplierDrawData
{
    internal static BuildingDrawData CreateDrawData()
    {
        var baseMeshPath = Main.Res.SubPath("multiplier.fbx");
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
            new MultiplierDrawData(),
            false,
            null,
            false);
    }
}

internal class MultiplierFactoryBuilder
    : IBuildingSimulationFactoryBuilder<MultiplierSimulation, LogicGate2In1OutSimulationState,
        EmptyCustomSimulationConfiguration>
{
    public IFactory<LogicGate2In1OutSimulationState, MultiplierSimulation> BuildFactory(
        SimulationSystemsDependencies dependencies,
        [UnscopedRef] out EmptyCustomSimulationConfiguration config)
    {
        config = new EmptyCustomSimulationConfiguration();
        return new MultiplierSimulationFactory();
    }
}

internal class MultiplierModuleDataProvider :
    SimulationBasedBuildingModuleDataProvider<MultiplierSimulation>
{
    protected override IEnumerable<IHUDSidePanelModuleData> GetSimulationModules(
        BuildingModel building,
        ILocalizedSimulation localizedSimulation,
        MultiplierSimulation actualSimulation)
    {
        yield return new HUDSidePanelModuleWireInfo.Data(
            "Input 1", actualSimulation.Input0Conductor);
        yield return new HUDSidePanelModuleWireInfo.Data(
            "Input 2", actualSimulation.Input1Conductor);
        yield return new HUDSidePanelModuleWireInfo.Data(
            "Output", actualSimulation.OutputConductor);
    }
}

internal class MultiplierSimulation(LogicGate2In1OutSimulationState state)
    : LogicGate2In1OutSimulation(state)
{
    protected override ISignal ComputeOutputSignal(ISignal a, ISignal b)
    {
        if (a is IntegerSignal sig1 && b is IntegerSignal sig2)
        {
            return IntegerSignal.Get(sig1.Value * sig2.Value);
        }

        return NullSignal.Instance;
    }
}

internal class MultiplierSimulationFactory :
    IFactory<LogicGate2In1OutSimulationState, MultiplierSimulation>
{
    public MultiplierSimulation Produce(LogicGate2In1OutSimulationState state)
    {
        return new MultiplierSimulation(state);
    }
}

internal class MultiplierSimulationRenderer(IMapModel map)
    : StatelessBuildingSimulationRenderer<MultiplierSimulation, IMultiplierDrawData>(map);

internal interface IMultiplierDrawData : IBuildingCustomDrawData;
