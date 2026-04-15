using System;
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

internal class DividerBuilding
{
    private readonly BuildingDefinitionId _defId = new("raphdf201-divider");
    private readonly ILogger _logger;

    internal DividerBuilding(ILogger logger)
    {
        _logger = logger;

        IBuildingGroupBuilder bldingGroup = BuildingGroup.Create(new BuildingDefinitionGroupId("raphdf201-divider-group"))
            .WithTitle("building-variant.raphdf201-divider.title".T())
            .WithDescription("building-variant.raphdf201-divider.description".T())
            .WithIcon(FileTextureLoader.LoadTextureAsSprite(Main.Res.SubPath("divider.png"), out _))
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
            .DynamicallyRendering<DividerSimulationRenderer, DividerSimulation, IDividerDrawData>(
                new DividerDrawData())
            .WithStaticDrawData(DividerDrawData.CreateDrawData())
            .WithoutSound()
            .WithoutSimulationConfiguration()
            .WithEfficiencyData(new BuildingEfficiencyData(2, 1));

        AtomicBuildings.Extend()
            .AllScenarios()
            .WithBuilding(blding, bldingGroup)
            .UnlockedAtMilestone(new ByIndexMilestoneSelector(2))
            .WithDefaultPlacement()
            .InToolbar(ToolbarElementLocator.Root().ChildAt(2).ChildAt(6).ChildAt(^1).InsertAfter())
            .WithSimulation(new DividerFactoryBuilder(), _logger)
            .WithCustomModules(new DividerModuleDataProvider())
            .WithoutPrediction()
            .Build();
    }

    internal AtomicStatefulBuildingSimulationSystem<DividerSimulation, LogicGate2In1OutSimulationState> Register()
    {
        return new AtomicStatefulBuildingSimulationSystem<DividerSimulation, LogicGate2In1OutSimulationState>(
            new DividerSimulationFactory(), _defId, _logger);
    }
}

internal class DividerDrawData : IDividerDrawData
{
    internal static BuildingDrawData CreateDrawData()
    {
        var baseMeshPath = Main.Res.SubPath("divider.fbx");
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
            new DividerDrawData(),
            false,
            null,
            false);
    }
}

internal class DividerFactoryBuilder
    : IBuildingSimulationFactoryBuilder<DividerSimulation, LogicGate2In1OutSimulationState,
        EmptyCustomSimulationConfiguration>
{
    public IFactory<LogicGate2In1OutSimulationState, DividerSimulation> BuildFactory(
        SimulationSystemsDependencies dependencies,
        [UnscopedRef] out EmptyCustomSimulationConfiguration config)
    {
        config = new EmptyCustomSimulationConfiguration();
        return new DividerSimulationFactory();
    }
}

internal class DividerModuleDataProvider :
    SimulationBasedBuildingModuleDataProvider<DividerSimulation>
{
    protected override IEnumerable<IHUDSidePanelModuleData> GetSimulationModules(
        BuildingModel building,
        ILocalizedSimulation localizedSimulation,
        DividerSimulation actualSimulation)
    {
        yield return new HUDSidePanelModuleWireInfo.Data(
            "Input 1", actualSimulation.Input0Conductor);
        yield return new HUDSidePanelModuleWireInfo.Data(
            "Input 2", actualSimulation.Input1Conductor);
        yield return new HUDSidePanelModuleWireInfo.Data(
            "Output", actualSimulation.OutputConductor);
    }
}

internal class DividerSimulation(LogicGate2In1OutSimulationState state)
    : LogicGate2In1OutSimulation(state)
{
    protected override ISignal ComputeOutputSignal(ISignal a, ISignal b)
    {
        if (a is IntegerSignal sig1 && b is IntegerSignal sig2)
        {
            return IntegerSignal.Get((int)Math.Floor((double)sig1.Value / sig2.Value));
        }

        return NullSignal.Instance;
    }
}

internal class DividerSimulationFactory :
    IFactory<LogicGate2In1OutSimulationState, DividerSimulation>
{
    public DividerSimulation Produce(LogicGate2In1OutSimulationState state)
    {
        return new DividerSimulation(state);
    }
}

internal class DividerSimulationRenderer(IMapModel map)
    : StatelessBuildingSimulationRenderer<DividerSimulation, IDividerDrawData>(map);

internal interface IDividerDrawData : IBuildingCustomDrawData;
