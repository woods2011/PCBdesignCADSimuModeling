using System;
using PCBdesignCADSimuModeling.Models.Technologies.PcbDesign;

namespace PCBdesignCADSimuModeling.Models.SimuSystem.SimulationEvents
{
    public class PcbDesignProcedureFinish : SimulationEvent
    {
        public PcbDesignProcedureFinish(PcbDesignTechnology pcbDesignTechnology) : base(1)
        {
            PcbDesignTechnology = pcbDesignTechnology;
            ActivateTime = pcbDesignTechnology.UpdateModelTime(TimeSpan.Zero);
        }

        public PcbDesignTechnology PcbDesignTechnology { get; }
    }
}