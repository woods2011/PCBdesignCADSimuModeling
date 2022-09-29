namespace PcbDesignSimuModeling.WPF.Models.Resources.CpuClusterRes;

public class CpuRequest : ResourceRequest<CpuCluster>
{
    private readonly int _reqThreadCount;
    private readonly (double avgOneThreadUtil, double onBaseClock)? _tuple;

    public CpuRequest(int requestId, int reqThreadCount, (double avgOneThreadUtil, double onBaseClock)? tuple = null) :
        base(requestId)
    {
        _reqThreadCount = reqThreadCount;
        _tuple = tuple;
    }

    protected override bool TryGetResourceBody(CpuCluster potentialResource) =>
        potentialResource.TryGetResource(RequestId, _reqThreadCount,
            _tuple is null
                ? 1.0
                : (_tuple.Value.onBaseClock / potentialResource.ClockRate) * _tuple.Value.avgOneThreadUtil);
}