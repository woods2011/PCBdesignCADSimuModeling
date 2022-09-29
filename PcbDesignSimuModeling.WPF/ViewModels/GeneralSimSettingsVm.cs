using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using MvvmDialogs.FrameworkDialogs.SaveFile;
using PcbDesignSimuModeling.WPF.Commands;
using PcbDesignSimuModeling.WPF.Models.SimuSystem;
using PcbDesignSimuModeling.WPF.ViewModels.Helpers;

namespace PcbDesignSimuModeling.WPF.ViewModels;

public class GeneralSimulationSettingsVm : BaseViewModel
{
    private readonly DialogService _dialogService = new();

    public int ExternalLoadThreadsCount { get; set; }
    public decimal ExternalLoadAvgOneThreadUtil { get; set; }
    public decimal ExternalLoadAvgOneThreadUtilReferenceClockRate { get; set; }
    public decimal ExternalLoadAvgRamUsage { get; set; }

    public int OsAvgThreadsCount { get; set; }
    public decimal OsAvgOneThreadUtil { get; set; }
    public decimal OsAvgOneThreadUtilReferenceClockRate { get; set; }
    public decimal OsAvgRamUsage { get; set; }
    public decimal OsLicensePricePerUserPrice { get; set; }
    public int OsAmortization { get; set; }

    public int MainEadAvgThreadsCount { get; set; }
    public decimal MainEadAvgOneThreadUtil { get; set; }
    public decimal MainEadOneThreadUtilReferenceClockRate { get; set; }
    public decimal MainEadAvgRamUsage { get; set; }
    public decimal MainEadLicensePricePerUser { get; set; }
    public int MainEadAmortization { get; set; }
    public int PlacingModuleAmortization { get; set; }
    public int RoutingModuleAmortization { get; set; }

    public int CpuAmortization { get; set; }
    public decimal OneSocketMBoardPrice { get; set; }
    public decimal TwoSocketMBoardPrice { get; set; }
    public int MBoardAmortization { get; set; }
    public decimal VirtualizationLicensePerCorePrice { get; set; }
    public int VirtualizationAmortization { get; set; }
    public decimal RamPerGigAvgPrice { get; set; }
    public int RamAmortization { get; set; }
    public decimal DiskPricePer1Gb { get; set; }
    public int DiskAmortization { get; set; }

    public decimal DesignerSalary { get; set; }
    public decimal ThinClientPrice { get; set; }
    public int ThinClientAmortization { get; set; }
    public decimal MonitorPrice { get; set; }
    public int MonitorAmortization { get; set; }
    public decimal DiskSpacePerUser { get; set; }

    public GeneralSimulationSettingsVm() => FromModel(GeneralSimulationSettings.CurSettings);

    public void FromModel(GeneralSimulationSettings settings)
    {
        ExternalLoadThreadsCount = settings.ExternalLoadThreadsCount;
        ExternalLoadAvgOneThreadUtil = settings.ExternalLoadAvgOneThreadUtil;
        ExternalLoadAvgOneThreadUtilReferenceClockRate = settings.ExternalLoadAvgOneThreadUtilReferenceClockRate;
        ExternalLoadAvgRamUsage = settings.ExternalLoadAvgRamUsage;
        
        OsAvgThreadsCount = settings.OsAvgThreadsCount;
        OsAvgOneThreadUtil = settings.OsAvgOneThreadUtil;
        OsAvgOneThreadUtilReferenceClockRate = settings.OsAvgOneThreadUtilReferenceClockRate;
        OsAvgRamUsage = settings.OsAvgRamUsage;
        OsLicensePricePerUserPrice = settings.OsLicensePricePerUserPrice;
        OsAmortization = settings.OsAmortization;

        MainEadAvgThreadsCount = settings.MainEadAvgThreadsCount;
        MainEadAvgOneThreadUtil = settings.MainEadAvgOneThreadUtil;
        MainEadOneThreadUtilReferenceClockRate = settings.MainEadOneThreadUtilReferenceClockRate;
        MainEadAvgRamUsage = settings.MainEadAvgRamUsage;
        MainEadLicensePricePerUser = settings.MainEadLicensePricePerUser;
        MainEadAmortization = settings.MainEadAmortization;
        PlacingModuleAmortization = settings.PlacingModuleAmortization;
        RoutingModuleAmortization = settings.RoutingModuleAmortization;

        CpuAmortization = settings.CpuAmortization;
        OneSocketMBoardPrice = settings.OneSocketMBoardPrice;
        TwoSocketMBoardPrice = settings.DualSocketMBoardPrice;
        MBoardAmortization = settings.MBoardAmortization;
        VirtualizationLicensePerCorePrice = settings.VirtualizationLicensePerCorePrice;
        VirtualizationAmortization = settings.VirtualizationAmortization;
        RamPerGigAvgPrice = settings.RamPerGigAvgPrice;
        RamAmortization = settings.RamAmortization;
        DiskPricePer1Gb = settings.DiskPricePer1Gb;
        DiskAmortization = settings.DiskAmortization;

        DesignerSalary = settings.DesignerSalary;
        ThinClientPrice = settings.ThinClientPrice;
        ThinClientAmortization = settings.ThinClientAmortization;
        MonitorPrice = settings.MonitorPrice;
        MonitorAmortization = settings.MonitorAmortization;
        DiskSpacePerUser = settings.DiskSpacePerUser;
    }

    public ICommand ImportCommand => new ActionCommand(_ => Import());
    public ICommand ExportCommand => new ActionCommand(_ => Export());

    public ICommand ResetToDefaultCommand => new ActionCommand(_ => ResetSettingsToDefault());

    public ICommand ResetToCurrentCommand => new ActionCommand(_ => ResetSettingsToCurrent());

    public ICommand ApplySettingsCommand => new ActionCommand(_ => ApplySettings());

    public void ResetSettingsToDefault()
    {
        var result = _dialogService.ShowMessageBox(this, "Вы уверены, что хотите сбросить настройки?", "Подтверждение!",
            MessageBoxButton.OKCancel, icon: MessageBoxImage.Question);

        if (result != MessageBoxResult.OK) return;

        GeneralSimulationSettings.ResetToDefaultSettings();
        FromModel(GeneralSimulationSettings.CurSettings);
    }

    public void ResetSettingsToCurrent() => FromModel(GeneralSimulationSettings.CurSettings);

    public void Import()
    {
        var dialogSettings = new OpenFileDialogSettings
        {
            Title = "Импорт настроек",
            InitialDirectory = Directory.GetCurrentDirectory(),
            Filter = "JSON file (*.json)|*.json|Text Documents (*.txt)|*.txt|All Files (*.*)|*.*"
        };

        var success = _dialogService.ShowOpenFileDialog(this, dialogSettings);
        if (success != true) return;

        var fileName = dialogSettings.FileName;
        try
        {
            var importedSettings = GeneralSimulationSettings.ImportSettingsOrDefault(fileName); // ToDo
            if (importedSettings is null) return;
            FromModel(importedSettings);
        }
        catch (FileNotFoundException fileNotFoundException)
        {
            _dialogService.ShowMessageBox(this, fileNotFoundException.Message, "Ошибка!", icon: MessageBoxImage.Error);
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
            _dialogService.ShowMessageBox(this, exception.Message, "Ошибка!", icon: MessageBoxImage.Error);
        }
    }

    public void Export()
    {
        var dialogSettings = new SaveFileDialogSettings()
        {
            Title = "Экспорт настроек",
            FileName = "GeneralSimulationSettings_Exported",
            InitialDirectory = Directory.GetCurrentDirectory(),
            Filter = "JSON file (*.json)|*.json|Text Documents (*.txt)|*.txt|All Files (*.*)|*.*"
        };

        var success = _dialogService.ShowSaveFileDialog(this, dialogSettings);
        if (success != true) return;

        var fileName = dialogSettings.FileName;
        GeneralSimulationSettings.ExportSettings(ToModel(), fileName);
    }

    public void ApplySettings()
    {
        GeneralSimulationSettings.ApplySettings(ToModel());
        FromModel(GeneralSimulationSettings.CurSettings); // ToDo: remove it
    }


    public GeneralSimulationSettings ToModel() => new()
    {
        ExternalLoadThreadsCount = ExternalLoadThreadsCount,
        ExternalLoadAvgOneThreadUtil = ExternalLoadAvgOneThreadUtil,
        ExternalLoadAvgOneThreadUtilReferenceClockRate = ExternalLoadAvgOneThreadUtilReferenceClockRate,
        ExternalLoadAvgRamUsage = ExternalLoadAvgRamUsage,

        OsAvgThreadsCount = OsAvgThreadsCount,
        OsAvgOneThreadUtil = OsAvgOneThreadUtil,
        OsAvgOneThreadUtilReferenceClockRate = OsAvgOneThreadUtilReferenceClockRate,
        OsAvgRamUsage = OsAvgRamUsage,
        OsLicensePricePerUserPrice = OsLicensePricePerUserPrice,
        OsAmortization = OsAmortization,

        MainEadAvgThreadsCount = MainEadAvgThreadsCount,
        MainEadAvgOneThreadUtil = MainEadAvgOneThreadUtil,
        MainEadOneThreadUtilReferenceClockRate = MainEadOneThreadUtilReferenceClockRate,
        MainEadAvgRamUsage = MainEadAvgRamUsage,
        MainEadLicensePricePerUser = MainEadLicensePricePerUser,
        MainEadAmortization = MainEadAmortization,
        PlacingModuleAmortization = PlacingModuleAmortization,
        RoutingModuleAmortization = RoutingModuleAmortization,

        CpuAmortization = CpuAmortization,
        OneSocketMBoardPrice = OneSocketMBoardPrice,
        DualSocketMBoardPrice = TwoSocketMBoardPrice,
        MBoardAmortization = MBoardAmortization,
        VirtualizationLicensePerCorePrice = VirtualizationLicensePerCorePrice,
        VirtualizationAmortization = VirtualizationAmortization,
        RamPerGigAvgPrice = RamPerGigAvgPrice,
        RamAmortization = RamAmortization,
        DiskPricePer1Gb = DiskPricePer1Gb,
        DiskAmortization = DiskAmortization,

        DesignerSalary = DesignerSalary,
        ThinClientPrice = ThinClientPrice,
        ThinClientAmortization = ThinClientAmortization,
        MonitorPrice = MonitorPrice,
        MonitorAmortization = MonitorAmortization,
        DiskSpacePerUser = DiskSpacePerUser
    };


    public override string Error => String.Empty;
    public override bool IsValid => true;
    public override string this[string columnName] => String.Empty;
}