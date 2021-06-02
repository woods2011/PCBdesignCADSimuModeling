using System;
using System.Collections.Generic;
using PCBdesignCADSimuModeling.Models.Exceptions;

namespace PCBdesignCADSimuModeling.Models.Resources.Algorithms.WireRoutingAlgorithms
{
    public static class WireRoutingAlgProviderFactory
    {
        private static readonly Dictionary<string, Func<IWireRoutingAlgFactory>> Map = new();

        
        static WireRoutingAlgProviderFactory()
        {
            Map["Example"] = () => IWireRoutingAlgFactory.ExampleWireRouting;
        }

        
        public static IWireRoutingAlgFactory Create(string wireRoutingAlgName)
        {
            var creator =
                GetCreator(wireRoutingAlgName) ?? throw new UnsupportedPcbAlgException(wireRoutingAlgName);

            return creator();
        }
        private static Func<IWireRoutingAlgFactory> GetCreator(string wireRoutingAlgName)
        {
            Map.TryGetValue(wireRoutingAlgName, out var creator);
            return creator;
        }
    }
}