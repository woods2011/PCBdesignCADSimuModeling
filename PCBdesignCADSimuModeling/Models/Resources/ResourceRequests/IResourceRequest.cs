using System;
using System.Collections.Generic;
using System.Linq;

namespace PcbDesignCADSimuModeling.Models.Resources.ResourceRequests
{
    public interface IResourceRequest
    {
        bool TryGetResource(List<IResource> availableResources, out IResource? reqResource);
    }
    
    
    public abstract class ResourceRequest<TResource> : IResourceRequest where TResource : IResource
    {
        protected ResourceRequest(int procId)
        {
            ProcId = procId;
        }

        
        public int ProcId { get; }
        
        
        public bool TryGetResource(List<IResource> availableResources, out IResource? reqResource)
        {
            reqResource = availableResources.FirstOrDefault(resource => resource is TResource tResource && TryGetResourceBody(tResource));
            return reqResource is not null;
        }

        protected abstract bool TryGetResourceBody(TResource potentialResource);
    }
}



// public bool TryGetResource(List<Resource> availableResources) =>
//     availableResources.FirstOrDefault(resource => resource is TResource) is TResource requestedResource
//     && TryGetResourceBody(availableResources, requestedResource);