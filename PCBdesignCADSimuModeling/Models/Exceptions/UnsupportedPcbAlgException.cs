using System;

namespace PCBdesignCADSimuModeling.Models.Exceptions
{
    public class UnsupportedPcbAlgException : Exception
    {
        public UnsupportedPcbAlgException(string wireRoutingAlgName) : base(
            $"Algorithm: {wireRoutingAlgName} is not Supported")
        {
        }
    }
}