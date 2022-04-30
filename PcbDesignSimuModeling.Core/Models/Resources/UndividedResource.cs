using System.ComponentModel;

namespace PcbDesignSimuModeling.Core.Models.Resources;

public abstract class UndividedResource : IResource
{
    protected int? UtilizingProcId;
    public abstract double ResValueForProc(int requestId);

    public abstract void FreeResource(int requestId);

    public abstract IResource Clone();
    public abstract double Cost { get; }
}


public class Designer : UndividedResource, INotifyPropertyChanged
{
    public Designer()
    { 
    }

    public bool TryGetResource(int procId)
    {
        if (UtilizingProcId.HasValue) return false;

        UtilizingProcId = procId;
        return true;
    }

    public override double ResValueForProc(int requestId) => 1.0;

    public override void FreeResource(int requestId) => UtilizingProcId = null;


    public override IResource Clone() => new Designer();

    public override double Cost => 60000;

    public event PropertyChangedEventHandler? PropertyChanged;
}