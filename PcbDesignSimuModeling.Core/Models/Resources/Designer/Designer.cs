using System.ComponentModel;

namespace PcbDesignSimuModeling.Core.Models.Resources.Designer;

public class Designer : UndividedResource, IPotentialFailureResource, INotifyPropertyChanged
{
    public bool TryGetResource(int procId)
    {
        if (UtilizingRequestId.HasValue || !IsActive) return false;

        UtilizingRequestId = procId;
        return true;
    }

    public bool IsActive { get; set; } = true;

    public override double PowerForRequest(int _) => 1.0;

    public override void FreeResource(int requestId) => UtilizingRequestId = null;


    public override IResource Clone() => new Designer();

    public override decimal Cost => 60000;

    public override string ToString() => "Проектировщик";

    public event PropertyChangedEventHandler? PropertyChanged;

}