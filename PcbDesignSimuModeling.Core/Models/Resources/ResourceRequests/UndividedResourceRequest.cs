namespace PcbDesignSimuModeling.Core.Models.Resources.ResourceRequests;

public abstract class UndividedResourceRequest<TResource> : ResourceRequest<TResource>
    where TResource : UndividedResource
{
    protected UndividedResourceRequest(int procId) : base(procId)
    {
    }
}


public class DesignerRequest : UndividedResourceRequest<Designer>
{
    public DesignerRequest(int procId) : base(procId)
    {
    }

    protected override bool TryGetResourceBody(Designer potentialResource) =>
        potentialResource.TryGetResource(ProcId);
}