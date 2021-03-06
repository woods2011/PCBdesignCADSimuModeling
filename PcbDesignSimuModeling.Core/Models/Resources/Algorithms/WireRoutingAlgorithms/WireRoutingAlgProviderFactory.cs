using PcbDesignSimuModeling.Core.Models.Exceptions;

namespace PcbDesignSimuModeling.Core.Models.Resources.Algorithms.WireRoutingAlgorithms;

public static class WireRoutingAlgProviderFactory
{
    private static readonly Dictionary<string, Func<IWireRoutingAlgFactory>> Map = new();

    static WireRoutingAlgProviderFactory()
    {
        Map[WireRoutingWaveStr] = () => new WireRoutingAlgFactory(WireRoutingOneThreadAlgorithm.WireRoutingWave);
        Map[WireRoutingChannelStr] =
            () => new WireRoutingAlgFactory(WireRoutingMultiThreadAlgorithm.WireRoutingChannel);

        AlgNameIndexMap[WireRoutingWaveStr] = 0;
        AlgIndexNameMap[0] = WireRoutingWaveStr;

        AlgNameIndexMap[WireRoutingChannelStr] = 1;
        AlgIndexNameMap[1] = WireRoutingChannelStr;
    }


    public static IWireRoutingAlgFactory Create(string wireRoutingAlgName) 
        => GetCreator(wireRoutingAlgName).Invoke();

    public static IWireRoutingAlgFactory Create(int wireRoutingAlgNameIndex)
    {
        AlgIndexNameMap.TryGetValue(wireRoutingAlgNameIndex, out var wireRoutingAlgName);
        _ = wireRoutingAlgName ?? throw new UnsupportedPcbAlgException(wireRoutingAlgNameIndex.ToString());
        
        return GetCreator(wireRoutingAlgName).Invoke();
    }

    private static Func<IWireRoutingAlgFactory> GetCreator(string wireRoutingAlgName)
    {
        Map.TryGetValue(wireRoutingAlgName, out var creator);
        return creator ?? throw new UnsupportedPcbAlgException(wireRoutingAlgName);
    }


    public const string WireRoutingWaveStr = "Волновой метод";
    public const string WireRoutingChannelStr = "Канальная (параллельная)";

    public static readonly Dictionary<string, int> AlgNameIndexMap = new();
    public static readonly Dictionary<int, string> AlgIndexNameMap = new();
}