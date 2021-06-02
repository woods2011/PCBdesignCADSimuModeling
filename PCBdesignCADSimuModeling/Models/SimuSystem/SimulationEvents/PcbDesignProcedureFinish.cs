using System;
using PCBdesignCADSimuModeling.Models.Technologies.PcbDesign;

namespace PCBdesignCADSimuModeling.Models.SimuSystem.SimulationEvents
{
    public class PcbDesignProcedureFinish : SimulationEvent
    {
        public PcbDesignProcedureFinish(PcbDesignTechnology pcbDesignTechnology, TimeSpan modelTime) : base(1)
        {
            PcbDesignTechnology = pcbDesignTechnology;
            ActivateTime = modelTime + pcbDesignTechnology.UpdateModelTime(TimeSpan.Zero);
        }

        public PcbDesignTechnology PcbDesignTechnology { get; }
    }
}