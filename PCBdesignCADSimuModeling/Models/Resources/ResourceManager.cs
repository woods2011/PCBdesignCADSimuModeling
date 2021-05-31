using System.Collections.Generic;

namespace PCBdesignCADSimuModeling.Models.Resources
{
    public class ResourceManager : IResourceManager
    {
        public bool TryGetResources(List<Resource> resources)
        {
            throw new System.NotImplementedException();
        }

        public void FreeResources(List<Resource> resources)
        {
            RebalanceResources();
            throw new System.NotImplementedException();
        }

        public void RebalanceResources()
        {
            throw new System.NotImplementedException();
        }
    }

    
    
    public interface IResourceManager
    {
        bool TryGetResources(List<Resource> resources);

        void FreeResources(List<Resource> resources);
    }
}