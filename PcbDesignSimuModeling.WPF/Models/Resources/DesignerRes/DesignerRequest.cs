namespace PcbDesignSimuModeling.WPF.Models.Resources.DesignerRes;

public class DesignerRequest : ResourceRequest<Designer>
{
    public DesignerRequest(int requestId) : base(requestId)
    {
    }

    protected override bool TryGetResourceBody(Designer potentialResource) =>
        potentialResource.TryGetResource(RequestId);
}