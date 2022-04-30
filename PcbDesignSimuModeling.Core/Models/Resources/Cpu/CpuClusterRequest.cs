namespace PcbDesignSimuModeling.Core.Models.Resources.Cpu;

public class CpuRequest : ResourceRequest<CpuCluster>
{
    private readonly int _reqThreadCount;
    private readonly double _avgOneThreadUtil;

    public CpuRequest(int requestId, int reqThreadCount, double avgOneThreadUtil = 1.0) : base(requestId)
    {
        _reqThreadCount = reqThreadCount;
        _avgOneThreadUtil = avgOneThreadUtil;
    }

    protected override bool TryGetResourceBody(CpuCluster potentialResource) =>
        potentialResource.TryGetResource(RequestId, _reqThreadCount, _avgOneThreadUtil);
}