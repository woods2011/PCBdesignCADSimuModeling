namespace PcbDesignSimuModeling.Core.Models.Resources.ResourceRequests;


public class ServerRequest : ResourceRequest<Server>
{
    public ServerRequest(int procId) : base(procId)
    {
    }

    protected override bool TryGetResourceBody(Server potentialResource) =>
        potentialResource.TryGetResource(ProcId);
}