using System.Diagnostics.CodeAnalysis;
using Core.Factory;
using ShapezShifter.Flow.Atomic;
using ShapezShifter.Hijack;

namespace MoreLogic;

internal class NAndGateFactoryBuilder
    : IFactoryBuilder<NAndGateSimulation, LogicGate2In1OutSimulationState, EmptyCustomSimulationConfiguration>
{
    public IFactory<LogicGate2In1OutSimulationState, NAndGateSimulation> BuildFactory(SimulationSystemsDependencies dependencies,
        [UnscopedRef] out EmptyCustomSimulationConfiguration config)
    {
        config = new EmptyCustomSimulationConfiguration();
        return new NAndGateSimulationFactory();
    }
}
