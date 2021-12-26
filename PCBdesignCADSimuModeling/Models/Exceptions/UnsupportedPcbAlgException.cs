using System;

namespace PcbDesignCADSimuModeling.Models.Exceptions
{
    public class UnsupportedPcbAlgException : Exception
    {
        public UnsupportedPcbAlgException(string algName) : base(
            $"Algorithm: {algName} is not Supported")
        {
        }
    }
}