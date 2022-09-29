using System;
using System.Collections.Generic;
using PcbDesignSimuModeling.WPF.Models.Exceptions;

namespace PcbDesignSimuModeling.WPF.Models.Resources.EadSybSystems.PlacingSystems;

public static class PlacingSysFactoryProvider
{
    private static readonly Dictionary<string, Func<IPlacingSysFactory>> Map = new();


    static PlacingSysFactoryProvider()
    {
        Map[PlacingToporStr] = () => new PlacingSysFactory(PlacingSystem.Topor);
        PlacingAlgNameIndexMap[PlacingToporStr] = 0;
        PlacingAlgIndexNameMap[0] = PlacingToporStr;
    }

    public static IPlacingSysFactory Create(string placingAlgName)
        => GetCreator(placingAlgName).Invoke();

    public static IPlacingSysFactory Create(int placingAlgIndex)
    {
        PlacingAlgIndexNameMap.TryGetValue(placingAlgIndex, out var placingAlgName);
        _ = placingAlgName ?? throw new UnsupportedPcbAlgException(placingAlgIndex.ToString());

        return GetCreator(placingAlgName).Invoke();
    }

    private static Func<IPlacingSysFactory> GetCreator(string placingAlgName)
    {
        Map.TryGetValue(placingAlgName, out var creator);
        return creator ?? throw new UnsupportedPcbAlgException(placingAlgName);
    }


    public const string PlacingToporStr = "TopoR";

    public static readonly Dictionary<string, int> PlacingAlgNameIndexMap = new();
    public static readonly Dictionary<int, string> PlacingAlgIndexNameMap = new();
}