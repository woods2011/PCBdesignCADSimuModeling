namespace PcbDesignSimuModeling.Core.Models.Resources.ResourceRequests;

public class CpuThreadRequest : ResourceRequest<CpuCluster>
{
    private readonly int _reqThreadCount;
    private readonly double _avgOneThreadUtil;

    public CpuThreadRequest(int procId, int reqThreadCount, double avgOneThreadUtil = 1.0) : base(procId)
    {
        _reqThreadCount = reqThreadCount;
        _avgOneThreadUtil = avgOneThreadUtil;
    }


    protected override bool TryGetResourceBody(CpuCluster potentialResource) =>
        potentialResource.TryGetResource(ProcId, _reqThreadCount, _avgOneThreadUtil);
}