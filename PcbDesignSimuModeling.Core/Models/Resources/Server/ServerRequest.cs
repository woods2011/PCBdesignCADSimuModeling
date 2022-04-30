namespace PcbDesignSimuModeling.Core.Models.Resources.Server;

public class ServerRequest : ResourceRequest<Server>
{
    public ServerRequest(int requestId) : base(requestId)
    {
    }

    protected override bool TryGetResourceBody(Server potentialResource) => potentialResource.TryGetResource(RequestId);
}