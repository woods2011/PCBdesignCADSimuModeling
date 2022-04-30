namespace PcbDesignSimuModeling.Core.Models.Resources.ResourceRequests;

public abstract class MixedResourceRequest<TMixedResources> : ResourceRequest<TMixedResources>
    where TMixedResources : MixedResource
{
    protected MixedResourceRequest(int procId) : base(procId)
    {
    }
}


public class CpuThreadRequest : MixedResourceRequest<CpuCluster>
{
    private readonly int _reqThreadCount;

        
    public CpuThreadRequest(int procId, int reqThreadCount) : base(procId)
    {
        _reqThreadCount = reqThreadCount;
    }


    protected override bool TryGetResourceBody(CpuCluster potentialResource) =>
        potentialResource.TryGetResource(ProcId, _reqThreadCount);
}