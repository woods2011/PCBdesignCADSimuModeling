using System.Collections.Generic;

namespace PCBdesignCADSimuModeling.Models.Resources.ResourceRequests
{
    public abstract class MixedResourceRequest<TMixedResources> : ResourceRequest<TMixedResources>
        where TMixedResources : MixedResource
    {
        protected override bool TryGetResourceBody(List<Resource> availableResources, TMixedResources requestedResource)
        {
            if (!TryGetResourceBodyBody(requestedResource, out var remainingResource)) return false;

            availableResources.Remove(requestedResource);
            if (remainingResource is not null)
                availableResources.Add(remainingResource);

            return true;
        }

        protected abstract bool TryGetResourceBodyBody(TMixedResources requestedResource,
            out TMixedResources remainingResource);
    }
}