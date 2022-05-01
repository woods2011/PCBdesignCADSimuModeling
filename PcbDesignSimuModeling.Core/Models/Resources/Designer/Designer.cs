using System.ComponentModel;

namespace PcbDesignSimuModeling.Core.Models.Resources.Designer;

public class Designer : UndividedResource, IPotentialFailureResource, INotifyPropertyChanged
{
    public bool TryGetResource(int procId)
    {
        if (UtilizingResourceId.HasValue || !IsActive) return false;

        UtilizingResourceId = procId;
        return true;
    }

    public bool IsActive { get; set; } = true;

    public override double PowerForRequest(int requestId) => 1.0;

    public override void FreeResource(int requestId) => UtilizingResourceId = null;


    public override IResource Clone() => new Designer();

    public override decimal Cost => 60000;

    public event PropertyChangedEventHandler? PropertyChanged;

}