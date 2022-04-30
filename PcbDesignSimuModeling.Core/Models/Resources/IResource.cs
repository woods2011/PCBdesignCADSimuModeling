using System.Text.Json.Serialization;

namespace PcbDesignSimuModeling.Core.Models.Resources;

public interface IResource
{
    public double ResValueForProc(int requestId);
    public void FreeResource(int requestId);
        
    public IResource Clone();
        
    [JsonIgnore]
    public double Cost { get; }
}

public interface IPotentialFailureResource
{
    [JsonIgnore]
    public bool IsActive { get; set; }
}