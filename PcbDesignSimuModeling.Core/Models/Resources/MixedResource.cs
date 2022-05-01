using MathNet.Numerics.LinearAlgebra.Double.Solvers;

namespace PcbDesignSimuModeling.Core.Models.Resources;

public abstract class MixedResource : IResource
{
    protected virtual List<int> UtilizingProcIds { get; } = new();
    public abstract double PowerForRequest(int requestId);
    public abstract void FreeResource(int requestId);
    public abstract IResource Clone();
    public abstract decimal Cost { get; }
}