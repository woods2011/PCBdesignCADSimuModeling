using System;
using System.Collections.Generic;
using PcbDesignSimuModeling.WPF.Models.Exceptions;

namespace PcbDesignSimuModeling.WPF.Models.Resources.EadSybSystems.RoutingSystems;

public static class RoutingSysFactoryProvider
{
    private static readonly Dictionary<string, Func<IRoutingSysFactory>> Map = new();

    static RoutingSysFactoryProvider()
    {
        Map[RoutingToporStr] = () => new RoutingSysFactory(RoutingSystem.Topor);

        RoutingAlgNameIndexMap[RoutingToporStr] = 0;
        RoutingAlgIndexNameMap[0] = RoutingToporStr;
    }


    public static IRoutingSysFactory Create(string wireRoutingAlgName)
        => GetCreator(wireRoutingAlgName).Invoke();

    public static IRoutingSysFactory Create(int wireRoutingAlgNameIndex)
    {
        RoutingAlgIndexNameMap.TryGetValue(wireRoutingAlgNameIndex, out var wireRoutingAlgName);
        _ = wireRoutingAlgName ?? throw new UnsupportedPcbAlgException(wireRoutingAlgNameIndex.ToString());

        return GetCreator(wireRoutingAlgName).Invoke();
    }

    private static Func<IRoutingSysFactory> GetCreator(string wireRoutingAlgName)
    {
        Map.TryGetValue(wireRoutingAlgName, out var creator);
        return creator ?? throw new UnsupportedPcbAlgException(wireRoutingAlgName);
    }

    public const string RoutingToporStr = "TopoR";

    public static readonly Dictionary<string, int> RoutingAlgNameIndexMap = new();
    public static readonly Dictionary<int, string> RoutingAlgIndexNameMap = new();
}