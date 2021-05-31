using System;
using PCBdesignCADSimuModeling.Models.Technologies.PcbDesign;

namespace PCBdesignCADSimuModeling.Models.SimuSystem.SimulationEvents
{
    public class PcbDesignTechnologyStart : SimulationEvent
    {
        public PcbDesignTechnologyStart(TimeSpan activateTime, PcbParams generatedPcb) : base(2, activateTime)
        {
            GeneratedPcb = generatedPcb;
        }

        public PcbParams GeneratedPcb { get; }
    }
}