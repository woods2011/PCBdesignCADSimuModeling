using System.Text.Json.Serialization;

namespace PcbDesignSimuModeling.Core.Models.Resources;

public interface IResource
{
    public double ResValueForProc(int procId);
    public void FreeResource(int procId);
        
    public IResource Clone();
        
    [JsonIgnore]
    public double Cost { get; }
}

public interface IPotentialFailureResource
{
    [JsonIgnore]
    public bool IsActive { get; set; }
}