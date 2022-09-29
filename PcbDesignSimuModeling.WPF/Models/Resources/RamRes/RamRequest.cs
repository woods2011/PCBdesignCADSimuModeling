namespace PcbDesignSimuModeling.WPF.Models.Resources.RamRes;

public class RamRequest : ResourceRequest<Ram>
{
    private readonly double _amount;

    public RamRequest(int requestId, double amount) : base(requestId) => _amount = amount;

    protected override bool TryGetResourceBody(Ram potentialResource) =>
        potentialResource.TryGetResource(RequestId, _amount);
}