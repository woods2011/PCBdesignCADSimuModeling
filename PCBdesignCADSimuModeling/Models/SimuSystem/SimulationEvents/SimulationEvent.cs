using System;

namespace PcbDesignCADSimuModeling.Models.SimuSystem.SimulationEvents
{
    public abstract class SimulationEvent
    {
        protected SimulationEvent(int priority, TimeSpan activateTime)
        {
            Priority = priority;
            ActivateTime = activateTime;
        }
        
        // protected SimulationEvent(int priority)
        // {
        //     Priority = priority;
        // }

        
        public TimeSpan ActivateTime { get; set; }
        public int Priority { get; }
    }
}