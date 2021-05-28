using System;
using System.Collections.Generic;
using PCBdesignCADSimuModeling.Models.Resources;

namespace PCBdesignCADSimuModeling.Models.Technologies.PcbDesign.ProjectProcedures
{
    public abstract class PcbDesignProcedure
    {
        private readonly PcbDesignTechnology _context;


        protected PcbDesignProcedure(PcbDesignTechnology context)
        {
            _context = context;
        }


        public List<Resource> Resources { get; }

        
        
        public TimeSpan UpdateModelTime(TimeSpan deltaTime)
        {
            UpdateModelTimeBody(deltaTime);
            return EstimateEndTime();
        }

        
        protected abstract void UpdateModelTimeBody(TimeSpan deltaTime);

        protected abstract TimeSpan EstimateEndTime();
    }
}