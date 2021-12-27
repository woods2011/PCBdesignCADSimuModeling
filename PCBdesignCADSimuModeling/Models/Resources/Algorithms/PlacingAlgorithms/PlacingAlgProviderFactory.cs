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
            
            AlgNameIndexMap[PlacingSequentialStr] = 0;
            AlgIndexNameMap[0] = PlacingSequentialStr;
            
            AlgNameIndexMap[PlacingPartitioningStr] = 1;
            AlgIndexNameMap[1] = PlacingPartitioningStr;
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
            return creator ?? throw new InvalidOperationException(nameof(placingAlgName));
        }


        public const string PlacingSequentialStr = "Последовательное размещение";
        public const string PlacingPartitioningStr = "Метод разбиения (параллельный)";
        
        public static readonly Dictionary<string, int> AlgNameIndexMap = new();
        public static readonly Dictionary<int, string> AlgIndexNameMap = new();
    }
}