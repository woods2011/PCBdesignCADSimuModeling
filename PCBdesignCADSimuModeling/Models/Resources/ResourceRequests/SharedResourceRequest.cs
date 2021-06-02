using System.Collections.Generic;

namespace PCBdesignCADSimuModeling.Models.Resources.ResourceRequests
{
    public abstract class SharedResourceRequest<TResource> : ResourceRequest<TResource>
        where TResource : SharedResource
    {
        protected override bool TryGetResourceBody(List<Resource> availableResources, TResource requestedResource) =>
            true;
    }
}