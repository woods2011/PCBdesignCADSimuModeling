using System.ComponentModel;

namespace PcbDesignSimuModeling.Core.Models.Resources.Designer;

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