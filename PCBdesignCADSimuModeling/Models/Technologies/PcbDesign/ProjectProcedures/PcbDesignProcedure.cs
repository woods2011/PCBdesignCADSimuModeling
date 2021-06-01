using System;
using System.Collections.Generic;
using PCBdesignCADSimuModeling.Models.Resources;

namespace PCBdesignCADSimuModeling.Models.Technologies.PcbDesign.ProjectProcedures
{
    public abstract class PcbDesignProcedure
    {
        protected readonly PcbDesignTechnology Context;
        

        protected PcbDesignProcedure(PcbDesignTechnology context)
        {
            Context = context;
        }

        
        public List<Resource> RequiredResources { get; } = new();
        
        
        

        public abstract bool NextProcedure();
        public abstract TimeSpan UpdateModelTime(TimeSpan deltaTime);
    }
}