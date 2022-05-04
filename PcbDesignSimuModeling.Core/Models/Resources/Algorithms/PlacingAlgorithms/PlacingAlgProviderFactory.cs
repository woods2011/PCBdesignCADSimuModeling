using PcbDesignSimuModeling.Core.Models.Exceptions;

namespace PcbDesignSimuModeling.Core.Models.Resources.Algorithms.PlacingAlgorithms;

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
        => GetCreator(placingAlgName).Invoke();

    public static IPlacingAlgFactory Create(int placingAlgIndex)
    {
        AlgIndexNameMap.TryGetValue(placingAlgIndex, out var placingAlgName);
        _ = placingAlgName ?? throw new UnsupportedPcbAlgException(placingAlgIndex.ToString());

        return GetCreator(placingAlgName).Invoke();
    }

    private static Func<IPlacingAlgFactory> GetCreator(string placingAlgName)
    {
        Map.TryGetValue(placingAlgName, out var creator);
        return creator ?? throw new UnsupportedPcbAlgException(placingAlgName);
    }


    public const string PlacingSequentialStr = "Последовательное размещение";
    public const string PlacingPartitioningStr = "Метод разбиения (параллельный)";

    public static readonly Dictionary<string, int> AlgNameIndexMap = new();
    public static readonly Dictionary<int, string> AlgIndexNameMap = new();
}