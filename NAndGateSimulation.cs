using Game.Content.Features.Signals;

namespace MoreLogic;

public class NAndGateSimulation(LogicGate2In1OutSimulationState state) : LogicGate2In1OutSimulation(state)
{
    protected override ISignal ComputeOutputSignal(ISignal a, ISignal b)
    {
        return IntegerSignal.Get(!(a.IsTruthy() && b.IsTruthy()));
    }
}
