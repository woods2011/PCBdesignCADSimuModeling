using System;
using PcbDesignCADSimuModeling.Models.Technologies.PcbDesign;

namespace PcbDesignCADSimuModeling.Models.SimuSystem.SimulationEvents
{
    public class PcbDesignProcedureFinish : SimulationEvent
    {
        public PcbDesignProcedureFinish(PcbDesignTechnology pcbDesignTechnology, TimeSpan activateTime) : base(1, activateTime)
        {
            PcbDesignTechnology = pcbDesignTechnology;
        }

        public PcbDesignTechnology PcbDesignTechnology { get; }
    }
}