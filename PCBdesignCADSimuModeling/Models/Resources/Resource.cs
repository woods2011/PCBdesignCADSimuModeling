using System;

namespace PCBdesignCADSimuModeling.Models.Resources
{
    public abstract class Resource
    {
        public abstract double ResValueForProc(Guid procId);
        public abstract void FreeResource(Guid procId);
    }
}