using System.Collections.Generic;

namespace PCBdesignCADSimuModeling.Models.Resources.ResourceRequests
{
    public abstract class UndividedResourceRequest<TResource> : ResourceRequest<TResource>
        where TResource : UndividedResource
    {
        protected override bool TryGetResourceBody(List<Resource> availableResources, TResource requestedResource) =>
            availableResources.Remove(requestedResource);
    }
    
    
    public class DesignerRequest : UndividedResourceRequest<Designer>
    {
    }
}