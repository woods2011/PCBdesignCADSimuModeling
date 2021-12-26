using System;
using Newtonsoft.Json;

namespace PcbDesignCADSimuModeling.Models.Resources
{
    public interface IResource
    {
        public double ResValueForProc(int procId);
        public void FreeResource(int procId);
        
        public IResource Clone();
        
        [JsonIgnore]
        public double Cost { get; }
    }
}