using static PcbDesignSimuModeling.WPF.Models.SimuSystem.GeneralSimulationSettings;

namespace PcbDesignSimuModeling.WPF.Models.Resources.DesignerRes;

public class Designer : UndividedResource, IPotentialFailureResource
{
    public bool IsActive { get; set; } = true;
    
    public bool TryGetResource(int procId)
    {
        if (UtilizingRequestId.HasValue || !IsActive) return false;

        UtilizingRequestId = procId;
        return true;
    }

    public override double PowerForRequest(int _) => 1.0;

    public override void FreeResource(int requestId) => UtilizingRequestId = null;


    public override decimal CostPerMonth { get; } = CurSettings.DesignerSalary;
    
    public override string ToString() => "Проектировщик";
}