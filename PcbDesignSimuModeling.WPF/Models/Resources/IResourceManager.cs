using System.Collections.Generic;
using System.Linq;

namespace PcbDesignSimuModeling.WPF.Models.Resources;

public interface IResourceManager
{
    bool TryGetResources(IEnumerable<IResourceRequest> resourceRequests,
        out List<(IResource Resource, int RequestId)> receivedResources);

    void FreeResources(List<(IResource Resource, int RequestId)> resources);
    public int NewRequestId { get; }
}

public class ResourceManager : IResourceManager
{
    private readonly IReadOnlyList<IResource> _resourcePool;

    public ResourceManager(IReadOnlyList<IResource> resourcePool) => _resourcePool = resourcePool;

    public bool TryGetResources(IEnumerable<IResourceRequest> resourceRequests,
        out List<(IResource Resource, int RequestId)> receivedResourcesOut)
    {
        var receivedResources = new List<(IResource Resource, int RequestId)>();
        var tempPool = _resourcePool.ToList();

        foreach (var resourceRequest in resourceRequests)
        {
            if (!resourceRequest.TryGetResource(tempPool, out var receivedResource))
            {
                receivedResources.ForEach(tuple => tuple.Resource.FreeResource(tuple.RequestId));
                receivedResourcesOut = new List<(IResource Resource, int RequestId)>();
                return false; // ToDo: можно добавить в лог тип не удовлетворенного запроса
            }

            if (receivedResource is UndividedResource) tempPool.Remove(receivedResource!);
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