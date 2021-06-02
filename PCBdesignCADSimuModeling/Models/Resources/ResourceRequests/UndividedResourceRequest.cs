using System;
using System.Collections.Generic;

namespace PCBdesignCADSimuModeling.Models.Resources.ResourceRequests
{
    public abstract class UndividedResourceRequest<TResource> : ResourceRequest<TResource>
        where TResource : UndividedResource
    {
        protected UndividedResourceRequest(Guid procId) : base(procId)
        {
        }
    }


    public class DesignerRequest : UndividedResourceRequest<Designer>
    {
        private readonly Designer.ExperienceEn _minDesignerExp;

        
        public DesignerRequest(Guid procId, Designer.ExperienceEn minDesignerExp = Designer.ExperienceEn.Little) :
            base(procId)
        {
            _minDesignerExp = minDesignerExp;
        }


        protected override bool TryGetResourceBody(Designer potentialResource) =>
            potentialResource.Experience >= _minDesignerExp && potentialResource.TryGetResource(ProcId);
    }
}