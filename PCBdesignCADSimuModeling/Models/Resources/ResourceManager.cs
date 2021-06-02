using System;
using System.Collections.Generic;
using PCBdesignCADSimuModeling.Models.Resources.ResourceRequests;

namespace PCBdesignCADSimuModeling.Models.Resources
{
    public class ResourceManager : IResourceManager
    {
        public bool TryGetResources(List<IResourceRequest> resourceRequests, out List<Resource> receivedResources)
        {
            receivedResources = new List<Resource>();
            
            foreach (var resourceRequest in resourceRequests)
            {
                if (!resourceRequest.TryGetResource(_resourcePool))
                {
                    foreach (var receivedResource in receivedResources)
                    {
                        //resource.Free(procId);
                    }
                    receivedResources = null;
                    return false;
                }
                    
            }

            return true;
        }

        public void FreeResources(List<Resource> resources)
        {
            Guid procId = new Guid();

            foreach (var resource in resources)
            {
                //resource.Free(procId);
            }
        }

        private readonly List<Resource> _resourcePool;

        public ResourceManager(List<Resource> resourcePool)
        {
            _resourcePool = resourcePool;
        }
    }

    
    
    public interface IResourceManager
    {
        bool TryGetResources(List<IResourceRequest> resourceRequests, out List<Resource> receivedResources);

        void FreeResources(List<Resource> resources);
    }
}