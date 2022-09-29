using System;
using PcbDesignSimuModeling.WPF.Models.Technologies.PcbDesign;

namespace PcbDesignSimuModeling.WPF.Models.SimuSystem.SimulationEvents;

public class PcbDesignProcedureFinish : SimulationEvent
{
    public PcbDesignProcedureFinish(TimeSpan activateTime, PcbDesignTechnology pcbDesignTechnology) :
        base(activateTime) => PcbDesignTechnology = pcbDesignTechnology;

    public PcbDesignTechnology PcbDesignTechnology { get; }
}