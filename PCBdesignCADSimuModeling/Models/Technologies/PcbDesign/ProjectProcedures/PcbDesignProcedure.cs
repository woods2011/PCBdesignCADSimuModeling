using System;
using System.Collections.Generic;
using PCBdesignCADSimuModeling.Models.Resources;
using PCBdesignCADSimuModeling.Models.Resources.ResourceRequests;

namespace PCBdesignCADSimuModeling.Models.Technologies.PcbDesign.ProjectProcedures
{
    public abstract class PcbDesignProcedure
    {
        protected readonly PcbDesignTechnology Context;


        protected PcbDesignProcedure(PcbDesignTechnology context)
        {
            Context = context;
        }


        public Guid ProcedureId { get; } = Guid.NewGuid();
        
        
        public List<IResourceRequest> RequiredResources { get; } = new();
        public List<IResource> ActiveResources { get; } = new();


        public abstract bool NextProcedure();
        public abstract TimeSpan UpdateModelTime(TimeSpan deltaTime);
    }
}