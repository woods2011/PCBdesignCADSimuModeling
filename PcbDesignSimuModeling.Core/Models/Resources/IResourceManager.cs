namespace PcbDesignSimuModeling.Core.Models.Resources;

public interface IResourceManager
{
    bool TryGetResources(List<IResourceRequest> resourceRequests,
        out List<(IResource Resource, int RequestId)> receivedResources);

    void FreeResources(List<(IResource Resource, int RequestId)> resources);
    public int NewRequestId { get; }
}

public class ResourceManager : IResourceManager
{
    private readonly List<IResource> _resourcePool;


    public ResourceManager(List<IResource> resourcePool)
    {
        _resourcePool = resourcePool;
    }


    public bool TryGetResources(List<IResourceRequest> resourceRequests,
        out List<(IResource Resource, int RequestId)> receivedResourcesOut)
    {
        var receivedResources = new List<(IResource Resource, int RequestId)>();
        var tempPool = _resourcePool.ToList();
        var poolIsTemped = false;

        foreach (var resourceRequest in resourceRequests)
        {
            if (!resourceRequest.TryGetResource(tempPool, out var receivedResource))
            {
                receivedResources.ForEach(tuple => tuple.Resource.FreeResource(resourceRequest.RequestId));
                receivedResourcesOut = new List<(IResource Resource, int RequestId)>();
                return false;
            }

            if (!poolIsTemped)
            {
                tempPool = _resourcePool.ToList();
                poolIsTemped = true;
            }

            tempPool.Remove(receivedResource!);
            receivedResources.Add((receivedResource!, resourceRequest.RequestId));
        }

        receivedResourcesOut = receivedResources;
        return true;
    }

    public void FreeResources(List<(IResource Resource, int RequestId)> resources) =>
        resources.ForEach(tuple => tuple.Resource.FreeResource(tuple.RequestId));
    
    public int NewRequestId => _curId++;
    private int _curId = 1;
}