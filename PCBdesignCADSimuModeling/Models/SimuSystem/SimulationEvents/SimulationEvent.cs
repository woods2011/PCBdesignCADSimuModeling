using System;

namespace PCBdesignCADSimuModeling.Models.SimuSystem.SimulationEvents
{
    public abstract class SimulationEvent
    {
        protected SimulationEvent(int priority, TimeSpan activateTime)
        {
            Priority = priority;
            ActivateTime = activateTime;
        }
        
        protected SimulationEvent(int priority)
        {
            Priority = priority;
        }

        
        public TimeSpan ActivateTime { get; set; } = TimeSpan.MaxValue / 2.0;
        public int Priority { get; }
    }
}