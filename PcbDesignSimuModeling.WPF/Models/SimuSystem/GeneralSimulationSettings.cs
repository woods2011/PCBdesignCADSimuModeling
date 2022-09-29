using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using PcbDesignSimuModeling.WPF.Models.Loggers;

namespace PcbDesignSimuModeling.WPF.Models.SimuSystem;

public class GeneralSimulationSettings
{
    public int ExternalLoadThreadsCount { get; init; } = 0;
    public decimal ExternalLoadAvgOneThreadUtil { get; init; } = 0;
    public decimal ExternalLoadAvgOneThreadUtilReferenceClockRate { get; init; } = 4;
    public decimal ExternalLoadAvgRamUsage { get; init; } = 0;

    public int OsAvgThreadsCount { get; init; } = 4;
    public decimal OsAvgOneThreadUtil { get; init; } = 0.1m;
    public decimal OsAvgOneThreadUtilReferenceClockRate { get; init; } = 4;
    public decimal OsAvgRamUsage { get; init; } = 2;
    public decimal OsLicensePricePerUserPrice { get; init; } = 100 * 79;
    public int OsAmortization { get; init; } = 1;


    public int MainEadAvgThreadsCount { get; init; } = 4;
    public decimal MainEadAvgOneThreadUtil { get; init; } = 0.3m;
    public decimal MainEadOneThreadUtilReferenceClockRate { get; init; } = 4;
    public decimal MainEadAvgRamUsage { get; init; } = 1.5m;
    public decimal MainEadLicensePricePerUser { get; init; } = 379905m;
    public int MainEadAmortization { get; init; } = 5;

    public int PlacingModuleAmortization { get; init; } = 5;
    public int RoutingModuleAmortization { get; init; } = 5;


    public int CpuAmortization { get; init; } = 4;
    public decimal OneSocketMBoardPrice { get; init; } = 450 * 79;
    public decimal DualSocketMBoardPrice { get; init; } = 800 * 79;
    public int MBoardAmortization { get; init; } = 5;
    public decimal VirtualizationLicensePerCorePrice { get; init; } = 0;
    public int VirtualizationAmortization { get; init; } = 1;
    public decimal RamPerGigAvgPrice { get; init; } = 450;
    public int RamAmortization { get; init; } = 5;


    public decimal DesignerSalary { get; init; } = 70000 / (1 - 0.4m);
    public decimal ThinClientPrice { get; init; } = 25000;
    public int ThinClientAmortization { get; init; } = 3;
    public decimal MonitorPrice { get; init; } = 12000;
    public int MonitorAmortization { get; init; } = 3;

    public decimal DiskSpacePerUser { get; init; } = 90;

    public decimal DiskPricePer1Gb { get; init; } = 30;

    public int DiskAmortization { get; init; } = 5;


    public GeneralSimulationSettings()
    {
    }


    public static GeneralSimulationSettings CurSettings { get; private set; }
    public static GeneralSimulationSettings DefaultSettings => new();

    private static readonly string SettingsPath = @$"{Directory.GetCurrentDirectory()}\GeneralSimulationSettings.json";

    private static readonly string SettingsBackUpPath =
        @$"{Directory.GetCurrentDirectory()}\GeneralSimulationSettingsBackUp.json";

    static GeneralSimulationSettings()
    {
        try
        {
            var importedSettings = ImportSettingsOrDefault(SettingsPath);
            if (importedSettings is not null)
            {
                CurSettings = importedSettings;
                return;
            }
        }
        catch (FileNotFoundException fileNotFoundException)
        {
            Debug.WriteLine(fileNotFoundException.Message);
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
        }

        CurSettings = DefaultSettings;
        ExportSettings(DefaultSettings, SettingsPath);
    }


    public static void ApplySettings(GeneralSimulationSettings settings)
    {
        if (CurSettings.Equals(obj: settings)) return;
        MakeBackUpOfCurrentSettings();
        CurSettings = settings;
        ExportSettings(settings, SettingsPath);
    }

    public static void ResetToDefaultSettings() => ApplySettings(DefaultSettings);

    public static GeneralSimulationSettings? ImportSettingsOrDefault(string filePath) =>
        JsonConvert.DeserializeObject<GeneralSimulationSettings?>(File.ReadAllText(filePath),
            new JsonSerializerSettings {MissingMemberHandling = MissingMemberHandling.Error});

    public static void ExportSettings(GeneralSimulationSettings settings, string filePath)
    {
        var serializedResult = JsonConvert.SerializeObject(settings, Formatting.Indented);
        new TruncateFileSimpleLogger(filePath).Log(serializedResult);
    }

    private static void MakeBackUpOfCurrentSettings() => ExportSettings(CurSettings, SettingsBackUpPath);


    protected bool Equals(GeneralSimulationSettings other)
    {
        return ExternalLoadThreadsCount == other.ExternalLoadThreadsCount &&
               ExternalLoadAvgOneThreadUtil == other.ExternalLoadAvgOneThreadUtil &&
               ExternalLoadAvgOneThreadUtilReferenceClockRate == other.ExternalLoadAvgOneThreadUtilReferenceClockRate &&
               ExternalLoadAvgRamUsage == other.ExternalLoadAvgRamUsage &&
               OsAvgThreadsCount == other.OsAvgThreadsCount && OsAvgOneThreadUtil == other.OsAvgOneThreadUtil &&
               OsAvgOneThreadUtilReferenceClockRate == other.OsAvgOneThreadUtilReferenceClockRate &&
               OsAvgRamUsage == other.OsAvgRamUsage && OsLicensePricePerUserPrice == other.OsLicensePricePerUserPrice &&
               OsAmortization == other.OsAmortization && MainEadAvgThreadsCount == other.MainEadAvgThreadsCount &&
               MainEadAvgOneThreadUtil == other.MainEadAvgOneThreadUtil &&
               MainEadOneThreadUtilReferenceClockRate == other.MainEadOneThreadUtilReferenceClockRate &&
               MainEadAvgRamUsage == other.MainEadAvgRamUsage &&
               MainEadLicensePricePerUser == other.MainEadLicensePricePerUser &&
               MainEadAmortization == other.MainEadAmortization &&
               PlacingModuleAmortization == other.PlacingModuleAmortization &&
               RoutingModuleAmortization == other.RoutingModuleAmortization &&
               CpuAmortization == other.CpuAmortization &&
               OneSocketMBoardPrice == other.OneSocketMBoardPrice &&
               DualSocketMBoardPrice == other.DualSocketMBoardPrice &&
               MBoardAmortization == other.MBoardAmortization &&
               VirtualizationLicensePerCorePrice == other.VirtualizationLicensePerCorePrice &&
               VirtualizationAmortization == other.VirtualizationAmortization &&
               RamPerGigAvgPrice == other.RamPerGigAvgPrice &&
               RamAmortization == other.RamAmortization &&
               DesignerSalary == other.DesignerSalary &&
               ThinClientPrice == other.ThinClientPrice &&
               ThinClientAmortization == other.ThinClientAmortization &&
               MonitorPrice == other.MonitorPrice &&
               MonitorAmortization == other.MonitorAmortization &&
               DiskSpacePerUser == other.DiskSpacePerUser &&
               DiskPricePer1Gb == other.DiskPricePer1Gb &&
               DiskAmortization == other.DiskAmortization;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((GeneralSimulationSettings) obj);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(ExternalLoadThreadsCount);
        hashCode.Add(ExternalLoadAvgOneThreadUtil);
        hashCode.Add(ExternalLoadAvgOneThreadUtilReferenceClockRate);
        hashCode.Add(ExternalLoadAvgRamUsage);
        hashCode.Add(OsAvgThreadsCount);
        hashCode.Add(OsAvgOneThreadUtil);
        hashCode.Add(OsAvgOneThreadUtilReferenceClockRate);
        hashCode.Add(OsAvgRamUsage);
        hashCode.Add(OsLicensePricePerUserPrice);
        hashCode.Add(OsAmortization);
        hashCode.Add(MainEadAvgThreadsCount);
        hashCode.Add(MainEadAvgOneThreadUtil);
        hashCode.Add(MainEadOneThreadUtilReferenceClockRate);
        hashCode.Add(MainEadAvgRamUsage);
        hashCode.Add(MainEadLicensePricePerUser);
        hashCode.Add(MainEadAmortization);
        hashCode.Add(PlacingModuleAmortization);
        hashCode.Add(RoutingModuleAmortization);
        hashCode.Add(CpuAmortization);
        hashCode.Add(OneSocketMBoardPrice);
        hashCode.Add(DualSocketMBoardPrice);
        hashCode.Add(MBoardAmortization);
        hashCode.Add(VirtualizationLicensePerCorePrice);
        hashCode.Add(VirtualizationAmortization);
        hashCode.Add(RamPerGigAvgPrice);
        hashCode.Add(RamAmortization);
        hashCode.Add(DesignerSalary);
        hashCode.Add(ThinClientPrice);
        hashCode.Add(ThinClientAmortization);
        hashCode.Add(MonitorPrice);
        hashCode.Add(MonitorAmortization);
        hashCode.Add(DiskSpacePerUser);
        hashCode.Add(DiskPricePer1Gb);
        hashCode.Add(DiskAmortization);
        return hashCode.ToHashCode();
    }
}