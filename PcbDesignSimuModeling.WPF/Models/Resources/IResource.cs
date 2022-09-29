using Newtonsoft.Json;

namespace PcbDesignSimuModeling.WPF.Models.Resources;

public interface IResource
{
    public double PowerForRequest(int requestId);
    public void FreeResource(int requestId);
    public  decimal CostPerMonth { get; }
}

public interface IPotentialFailureResource
{
    public bool IsActive { get; set; }
}