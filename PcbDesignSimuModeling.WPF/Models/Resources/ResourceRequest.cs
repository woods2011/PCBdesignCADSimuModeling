using System.Collections.Generic;
using System.Linq;

namespace PcbDesignSimuModeling.WPF.Models.Resources;

public interface IResourceRequest
{
    bool TryGetResource(List<IResource> availableResources, out IResource? reqResource);
    
    public int RequestId { get; }
}

public abstract class ResourceRequest<TResource> : IResourceRequest where TResource : IResource
{
    protected ResourceRequest(int requestId) => RequestId = requestId;

    public int RequestId { get; }

    public virtual bool TryGetResource(List<IResource> availableResources, out IResource? reqResource)
    {
        reqResource = availableResources.OfType<TResource>().FirstOrDefault(TryGetResourceBody);
        return reqResource is not null;
    }

    protected abstract bool TryGetResourceBody(TResource potentialResource);
}