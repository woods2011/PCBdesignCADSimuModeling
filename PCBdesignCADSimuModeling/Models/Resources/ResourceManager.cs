using System;
using System.Collections.Generic;
using System.Linq;
using PCBdesignCADSimuModeling.Models.Resources.ResourceRequests;

namespace PCBdesignCADSimuModeling.Models.Resources
{
    public class ResourceManager : IResourceManager
    {
        private readonly List<Resource> _resourcePool;


        public ResourceManager(List<Resource> resourcePool)
        {
            _resourcePool = resourcePool;
        }


        public bool TryGetResources(Guid procId, List<IResourceRequest> resourceRequests,
            out List<Resource> receivedResourcesOut)
        {
            var receivedResources = new List<Resource>();

            foreach (var resourceRequest in resourceRequests)
            {
                if (!resourceRequest.TryGetResource(
                    _resourcePool.Where(resource => !receivedResources.Contains(resource)).ToList(),
                    out var receivedResource))
                {
                    receivedResources.ForEach(resource => resource.FreeResource(procId));
                    receivedResourcesOut = new List<Resource>();
                    return false;
                }

                receivedResources.Add(receivedResource);
            }

            receivedResourcesOut = receivedResources;
            return true;
        }

        public void FreeResources(Guid procId, List<Resource> resources) =>
            resources.ForEach(resource => resource.FreeResource(procId));
    }


    public interface IResourceManager
    {
        bool TryGetResources(Guid procId, List<IResourceRequest> resourceRequests,
            out List<Resource> receivedResources);

        void FreeResources(Guid procId, List<Resource> resources);
    }
}