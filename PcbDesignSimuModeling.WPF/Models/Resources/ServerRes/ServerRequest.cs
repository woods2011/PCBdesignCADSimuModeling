namespace PcbDesignSimuModeling.WPF.Models.Resources.ServerRes;

public class ServerRequest : ResourceRequest<Server>
{
    public ServerRequest(int requestId) : base(requestId)
    {
    }

    protected override bool TryGetResourceBody(Server potentialResource) => potentialResource.TryGetResource(RequestId);
}