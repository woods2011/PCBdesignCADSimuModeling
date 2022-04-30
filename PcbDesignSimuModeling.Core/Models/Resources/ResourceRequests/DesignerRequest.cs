namespace PcbDesignSimuModeling.Core.Models.Resources.ResourceRequests;


public class DesignerRequest : ResourceRequest<Designer>
{
    public DesignerRequest(int procId) : base(procId)
    {
    }

    protected override bool TryGetResourceBody(Designer potentialResource) =>
        potentialResource.TryGetResource(ProcId);
}