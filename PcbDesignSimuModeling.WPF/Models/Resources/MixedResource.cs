using System.Collections.Generic;

namespace PcbDesignSimuModeling.WPF.Models.Resources;

public abstract class MixedResource : IResource
{
    protected virtual List<int> UtilizingRequestsIds { get; } = new();

    public abstract double PowerForRequest(int requestId);
    public abstract void FreeResource(int requestId);
    public abstract decimal CostPerMonth { get; }
}