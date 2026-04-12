using Core.Factory;

namespace MoreLogic;

public class NAndGateSimulationFactory :
    IFactory<LogicGate2In1OutSimulationState, NAndGateSimulation>
{
    public NAndGateSimulation Produce(LogicGate2In1OutSimulationState state)
    {
        return new NAndGateSimulation(state);
    }
}
