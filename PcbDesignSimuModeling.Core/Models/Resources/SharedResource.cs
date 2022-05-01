namespace PcbDesignSimuModeling.Core.Models.Resources;

public abstract class SharedResource : IResource
{
    public abstract double PowerForRequest(int requestId);

    public abstract void FreeResource(int requestId);
    public abstract IResource Clone();
    public abstract decimal Cost { get; }
}