using System;
using System.Collections.Generic;

namespace PCBdesignCADSimuModeling.Models.Resources.ResourceRequests
{
    public abstract class MixedResourceRequest<TMixedResources> : ResourceRequest<TMixedResources>
        where TMixedResources : MixedResource
    {
        protected MixedResourceRequest(Guid procId) : base(procId)
        {
        }
    }


    public class CpuThreadRequest : MixedResourceRequest<CpuThreads>
    {
        private readonly int _reqThreadCount;

        
        public CpuThreadRequest(Guid procId, int reqThreadCount) : base(procId)
        {
            _reqThreadCount = reqThreadCount;
        }


        protected override bool TryGetResourceBody(CpuThreads potentialResource) =>
            potentialResource.TryGetResource(ProcId, _reqThreadCount);
    }
}