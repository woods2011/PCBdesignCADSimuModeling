using PcbDesignSimuModeling.Core.Models.Technologies.PcbDesign;

namespace PcbDesignSimuModeling.Core.Models.SimuSystem.SimulationEvents;

public class PcbDesignTechnologyStart : SimulationEvent
{
    public PcbDesignTechnologyStart(TimeSpan activateTime, PcbParams generatedPcb) : base(activateTime) =>
        GeneratedPcb = generatedPcb;

    public PcbParams GeneratedPcb { get; }
}