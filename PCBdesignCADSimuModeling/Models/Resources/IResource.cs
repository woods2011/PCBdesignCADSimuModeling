using System;
using Newtonsoft.Json;

namespace PCBdesignCADSimuModeling.Models.Resources
{
    public interface IResource
    {
        public double ResValueForProc(Guid procId);
        public void FreeResource(Guid procId);
        
        public IResource Clone();
        
        [JsonIgnore]
        public double Cost { get; }
    }
}