using System;
using System.Collections.Generic;
using System.Linq;
using PcbDesignCADSimuModeling.Models.Resources.ResourceRequests;

namespace PcbDesignCADSimuModeling.Models.Resources
{
    public class ResourceManager : IResourceManager
    {
        private readonly List<IResource> _resourcePool;


        public ResourceManager(List<IResource> resourcePool)
        {
            _resourcePool = resourcePool;
        }


        public bool TryGetResources(int procId, List<IResourceRequest> resourceRequests,
            out List<IResource> receivedResourcesOut)
        {
            var receivedResources = new List<IResource>();
            var tempPool = _resourcePool.ToList();
            var poolIsTemped = false;
            
            foreach (var resourceRequest in resourceRequests)
            {
                if (!resourceRequest.TryGetResource(tempPool, out var receivedResource))
                {
                    receivedResources.ForEach(resource => resource.FreeResource(procId));
                    receivedResourcesOut = new List<IResource>();
                    return false;
                }
            
                if (!poolIsTemped)
                {
                    tempPool = _resourcePool.ToList();
                    poolIsTemped = true;
                }
                
                tempPool.Remove(receivedResource!);
                receivedResources.Add(receivedResource!);
            }
            
            receivedResourcesOut = receivedResources;
            return true;
        }

        public void FreeResources(int procId, List<IResource> resources) =>
            resources.ForEach(resource => resource.FreeResource(procId));
    }


    public interface IResourceManager
    {
        bool TryGetResources(int procId, List<IResourceRequest> resourceRequests,
            out List<IResource> receivedResources);

        void FreeResources(int procId, List<IResource> resources);
    }
}