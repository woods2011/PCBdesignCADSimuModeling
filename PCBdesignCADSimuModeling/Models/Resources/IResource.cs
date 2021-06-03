using System;

namespace PCBdesignCADSimuModeling.Models.Resources
{
    public interface IResource
    {
        public double ResValueForProc(Guid procId);
        public void FreeResource(Guid procId);
    }
}