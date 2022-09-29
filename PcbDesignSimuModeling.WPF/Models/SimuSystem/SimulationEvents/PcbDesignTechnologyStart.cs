using System;
using PcbDesignSimuModeling.WPF.Models.Technologies.PcbDesign;

namespace PcbDesignSimuModeling.WPF.Models.SimuSystem.SimulationEvents;

public class PcbDesignTechnologyStart : SimulationEvent
{
    public PcbDesignTechnologyStart(TimeSpan activateTime, PcbDescription generatedPcb) : base(activateTime) =>
        GeneratedPcb = generatedPcb;

    public PcbDescription GeneratedPcb { get; }
}