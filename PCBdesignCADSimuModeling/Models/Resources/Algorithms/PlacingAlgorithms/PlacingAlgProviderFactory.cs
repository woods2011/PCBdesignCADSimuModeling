using System;
using System.Collections.Generic;
using PCBdesignCADSimuModeling.Models.Exceptions;

namespace PCBdesignCADSimuModeling.Models.Resources.Algorithms.PlacingAlgorithms
{
    public static class PlacingAlgProviderFactory
    {
        private static readonly Dictionary<string, Func<IPlacingAlgFactory>> Map = new();

        
        static PlacingAlgProviderFactory()
        {
            Map["Example"] = () =>
                new PlacingAlgFactory(pcbParams =>
                    new PlacingMultiThreadAlgorithm(new PlacingExampleCxtyEst(pcbParams), 8, 0.7));
        }

        
        public static IPlacingAlgFactory Create(string placingAlgName)
        {
            var creator =
                GetCreator(placingAlgName) ?? throw new UnsupportedPcbAlgException(placingAlgName);

            return creator();
        }
        private static Func<IPlacingAlgFactory> GetCreator(string placingAlgName)
        {
            Map.TryGetValue(placingAlgName, out var creator);
            return creator;
        }
    }
}