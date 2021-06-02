using System.Collections.Generic;
using System.Linq;

namespace PCBdesignCADSimuModeling.Models.Resources
{
    public abstract class ResourceRequest<TResource> where TResource : Resource
    {
        public bool TryGetResource(List<Resource> availableResources) =>
            availableResources.FirstOrDefault(resource => resource is TResource) is TResource requestedResource
            && TryGetResourceBody(availableResources, requestedResource);

        protected abstract bool TryGetResourceBody(List<Resource> availableResources, TResource requestedResource);
    }


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


    public class UndividedResourceRequest<TResource> : ResourceRequest<TResource>
        where TResource : UndividedResource
    {
        protected override bool TryGetResourceBody(List<Resource> availableResources, TResource requestedResource) =>
            availableResources.Remove(requestedResource);
    }


    public class SharedResourceRequest<TResource> : ResourceRequest<TResource>
        where TResource : SharedResource
    {
        protected override bool TryGetResourceBody(List<Resource> availableResources, TResource requestedResource) =>
            true;
    }
}