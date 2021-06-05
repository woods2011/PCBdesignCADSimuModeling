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
            Map[WireRoutingWaveStr] = () => IWireRoutingAlgFactory.WireRoutingWave;
            Map[WireRoutingChannelStr] = () => IWireRoutingAlgFactory.WireRoutingChannel;
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
        
        
        public static string WireRoutingWaveStr = "Волновой метод";
        public static string WireRoutingChannelStr = "Канальный (параллельный)";
    }
}