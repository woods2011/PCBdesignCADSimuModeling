using System;
using PcbDesignCADSimuModeling.Models.Technologies.PcbDesign;

namespace PcbDesignCADSimuModeling.Models.SimuSystem.SimulationEvents
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