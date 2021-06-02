﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace PCBdesignCADSimuModeling.Models.Resources.ResourceRequests
{
    public interface IResourceRequest
    {
        bool TryGetResource(List<Resource> availableResources, out Resource reqResource);
    }
    
    
    public abstract class ResourceRequest<TResource> : IResourceRequest where TResource : Resource
    {
        protected ResourceRequest(Guid procId)
        {
            ProcId = procId;
        }

        
        public Guid ProcId { get; }
        
        
        public bool TryGetResource(List<Resource> availableResources, out Resource reqResource)
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