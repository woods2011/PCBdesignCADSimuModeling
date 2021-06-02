using System.Collections.Generic;
using System.Linq;

namespace PCBdesignCADSimuModeling.Models.Resources.ResourceRequests
{
    public interface IResourceRequest
    {
        bool TryGetResource(List<Resource> availableResources);
    }
    
    
    public abstract class ResourceRequest<TResource> : IResourceRequest where TResource : Resource
    {
        public bool TryGetResource(List<Resource> availableResources) =>
            availableResources.FirstOrDefault(resource => resource is TResource) is TResource requestedResource
            && TryGetResourceBody(availableResources, requestedResource);

        protected abstract bool TryGetResourceBody(List<Resource> availableResources, TResource requestedResource);
    }
}