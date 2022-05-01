using System.Text.Json.Serialization;

namespace PcbDesignSimuModeling.Core.Models.Resources;

public interface IResource
{
    public double PowerForRequest(int requestId);
    public void FreeResource(int requestId);
        
    public IResource Clone();
        
    [JsonIgnore]
    public decimal Cost { get; }
}

public interface IPotentialFailureResource
{
    [JsonIgnore]
    public bool IsActive { get; set; }
}