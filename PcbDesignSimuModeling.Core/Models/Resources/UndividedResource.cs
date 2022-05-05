namespace PcbDesignSimuModeling.Core.Models.Resources;

public abstract class UndividedResource : IResource
{
    public bool IsBusy => UtilizingRequestId is not null;
    
    protected int? UtilizingRequestId;
    public abstract double PowerForRequest(int requestId);

    public abstract void FreeResource(int requestId);
    
    public abstract decimal Cost { get; }
    public abstract IResource Clone();
}