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

        
        public List<Resource> Resources { get; protected init; } = new();
        

        public abstract bool NextProcedure();
        public abstract TimeSpan UpdateModelTime(TimeSpan deltaTime);
    }
}