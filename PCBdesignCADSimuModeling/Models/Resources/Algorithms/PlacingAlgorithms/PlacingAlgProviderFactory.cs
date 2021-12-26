using System;
using System.Collections.Generic;
using PcbDesignCADSimuModeling.Models.Exceptions;

namespace PcbDesignCADSimuModeling.Models.Resources.Algorithms.PlacingAlgorithms
{
    public static class PlacingAlgProviderFactory
    {
        private static readonly Dictionary<string, Func<IPlacingAlgFactory>> Map = new();


        static PlacingAlgProviderFactory()
        {
            Map[PlacingSequentialStr] = () => new PlacingAlgFactory(PlacingOneThreadAlgorithm.PlacingSequential);
            Map[PlacingPartitioningStr] = () => new PlacingAlgFactory(PlacingMultiThreadAlgorithm.PlacingPartitioning);
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

        
        public static string PlacingSequentialStr = "Последовательное размещение";
        public static string PlacingPartitioningStr = "Метод разбиения (параллельный)";
    }
}