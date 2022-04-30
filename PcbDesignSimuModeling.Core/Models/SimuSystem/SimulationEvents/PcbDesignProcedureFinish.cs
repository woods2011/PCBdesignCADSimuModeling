using PcbDesignSimuModeling.Core.Models.Technologies.PcbDesign;

namespace PcbDesignSimuModeling.Core.Models.SimuSystem.SimulationEvents;

public class PcbDesignProcedureFinish : SimulationEvent
{
    public PcbDesignProcedureFinish(TimeSpan activateTime, PcbDesignTechnology pcbDesignTechnology) :
        base(activateTime) => PcbDesignTechnology = pcbDesignTechnology;

    public PcbDesignTechnology PcbDesignTechnology { get; }
}