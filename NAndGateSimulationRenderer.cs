namespace MoreLogic;

public class NAndGateSimulationRenderer(IMapModel map)
    : StatelessBuildingSimulationRenderer<NAndGateSimulation, INAndGateDrawData>(map);
