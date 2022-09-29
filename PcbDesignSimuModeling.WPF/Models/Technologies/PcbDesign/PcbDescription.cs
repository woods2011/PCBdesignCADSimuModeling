using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using PcbDesignSimuModeling.WPF.Models.Loggers;

namespace PcbDesignSimuModeling.WPF.Models.Technologies.PcbDesign;

public class PcbDescription
{
    public PcbDescription(int elementsCount, double newElemsPercent, double elementsDensity, int pinsCount,
        double pinDensity, int numberOfLayers, bool isDoubleSidePlacement, bool isManualSchemeInput,
        int classAccuracy)
    {
        ElementsCount = Math.Clamp(elementsCount, 20, 1100);
        NewElemsPercent = Math.Clamp(newElemsPercent, 0.0, 1.0);
        ElementsDensity = Math.Clamp(elementsDensity, 0.05, 1.9);
        PinsCount = Math.Clamp(pinsCount, 80, 10000);
        PinDensity = Math.Clamp(pinDensity, 0.01, 20);
        NumberOfLayers = Math.Max(1, numberOfLayers <= 2
            ? numberOfLayers
            : (numberOfLayers % 2 == 0 ? numberOfLayers : numberOfLayers + 1));
        IsDoubleSidePlacement = isDoubleSidePlacement;
        IsManualSchemeInput = isManualSchemeInput;
        ClassAccuracy = Math.Clamp(classAccuracy, 1, 5);
    }

    [JsonProperty("Количество Элементов")] public int ElementsCount { get; }

    [JsonProperty("Процент Элементов Отсутсвующих в Библиотеке")]
    public double NewElemsPercent { get; }

    [JsonProperty("Коэффициент Покрытия Элементами")]
    public double ElementsDensity { get; }

    [JsonProperty("Количество Контактов")] public int PinsCount { get; }

    [JsonProperty("Плотность Контактов")] public double PinDensity { get; }

    [JsonProperty("Количество слоев")] public int NumberOfLayers { get; }

    [JsonProperty("Возможность размещения с двух сторон")]
    public bool IsDoubleSidePlacement { get; }

    [JsonProperty("Ручной ввод принципиальной схемы")]
    public bool IsManualSchemeInput { get; }

    [JsonProperty("Класс точности печатной платы")]
    public int ClassAccuracy { get; }


    [JsonIgnore] public double PcbArea => PinsCount / PinDensity;


    [JsonProperty("Название")] public string Name { get; init; } = String.Empty;

    public static List<PcbDescription>? ImportPcbDescriptions(string filePath) =>
        JsonConvert.DeserializeObject<List<PcbDescription>?>(File.ReadAllText(filePath));

    public static void ExportPcbDescriptions(IEnumerable<PcbDescription> descriptions, string filePath)
    {
        var serializedResult = JsonConvert.SerializeObject(descriptions, Formatting.Indented);
        new AppendFileSimpleLogger(filePath).Log(serializedResult);
    }
}