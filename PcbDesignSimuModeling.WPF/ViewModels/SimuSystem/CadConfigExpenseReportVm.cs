using System;
using System.ComponentModel;
using PcbDesignSimuModeling.WPF.Models;
using static PcbDesignSimuModeling.WPF.Models.SimuSystem.GeneralSimulationSettings;

namespace PcbDesignSimuModeling.WPF.ViewModels.SimuSystem;

public class CadConfigExpenseReportVm : INotifyPropertyChanged
{
    private readonly CadConfigurationVm _vm;

    public CadConfigExpenseReportVm(CadConfigurationVm vm)
    {
        _vm = vm;
        _vm.PropertyChanged += VmOnPropertyChanged;
        _vm.Cpu.PropertyChanged += VmOnPropertyChanged;
        _vm.Server.PropertyChanged += VmOnPropertyChanged;
        _vm.Ram.PropertyChanged += VmOnPropertyChanged;

        RecalculateConfig();
    }

    private void VmOnPropertyChanged(object? sender, PropertyChangedEventArgs e) => RecalculateConfig();

    public void RecalculateConfig()
    {
        var cpu = _vm.Cpu.Model;
        var ram = _vm.Ram.Model;
        var server = _vm.Server.Model;

        string configStr;

        var totalCpuRelatedCosts = 0m;
        if (cpu.CoveringConfig is not null)
        {
            var (cpuModel, isDualSocket) = cpu.CoveringConfig.Value;

            var cpuCostPerM = cpu.CostPerMonth;
            var twoSocMBoardPricePerM =
                CurSettings.DualSocketMBoardPrice.WithMonthAmort(CurSettings.MBoardAmortization);
            var oneSocMBoardPricePerM = CurSettings.OneSocketMBoardPrice.WithMonthAmort(CurSettings.MBoardAmortization);
            var virtualizationPricePerM =
                cpuModel.ThreadCount / 2m *
                CurSettings.VirtualizationLicensePerCorePrice.WithMonthAmort(CurSettings.VirtualizationAmortization);
            totalCpuRelatedCosts = (isDualSocket ? twoSocMBoardPricePerM : oneSocMBoardPricePerM) +
                                   virtualizationPricePerM + cpuCostPerM;

            configStr =
                $"   Расходы зависящие от Вычислительного Сервера:{Environment.NewLine}" +
                $"      ЦП: {cpuModel.Name} ({cpuModel.ThreadCount}x{cpuModel.ClockRate:F2})≈({cpuModel.ThreadCount * cpuModel.ClockRate / cpu.ClockRate:F0}x{cpu.ClockRate:F2});{Environment.NewLine}" +
                $"          Количество: {(isDualSocket ? 2 : 1)}; Расходы на еденицу: {(cpuCostPerM / (isDualSocket ? 2 : 1)):N1}{Environment.NewLine}" +
                $"      Материнская плата: {(isDualSocket ? "Двухсокетная;" : "ОдноСокетная;")} Расходы: {(isDualSocket ? $"{twoSocMBoardPricePerM:N1}" : $"{oneSocMBoardPricePerM:N1}")}{Environment.NewLine}" +
                $"      Расходы на средства виртуализации: {virtualizationPricePerM:N1}{Environment.NewLine}" +
                $"   ИТОГ: {totalCpuRelatedCosts:N1}{Environment.NewLine}{Environment.NewLine}";
        }
        else
        {
            configStr =
                $"   Расходы зависящие от Вычислительного Сервера:{Environment.NewLine}" +
                $"      Расчет затрат не произведен, т.к. конфигурация не осуществима!{Environment.NewLine}{Environment.NewLine}";
        }


        var ramCostPerM = ram.CostPerMonth;
        var serverCostPerM = server.CostPerMonth;

        var designersCount = _vm.DesignersCount;
        var designerSalary = designersCount * CurSettings.DesignerSalary;
        var oSLicensePricePerM = designersCount *
                                 CurSettings.OsLicensePricePerUserPrice.WithMonthAmort(CurSettings.OsAmortization);
        var mainEadPricePerM = designersCount *
                               CurSettings.MainEadLicensePricePerUser.WithMonthAmort(CurSettings.MainEadAmortization);
        var diskSpacePricePerM = designersCount * CurSettings.DiskSpacePerUser *
                                 CurSettings.DiskPricePer1Gb.WithMonthAmort(CurSettings.DiskAmortization);
        var thinClientPricePerM =
            designersCount * CurSettings.ThinClientPrice.WithMonthAmort(CurSettings.ThinClientAmortization);
        var monitorPricePerM =
            designersCount * CurSettings.MonitorPrice.WithMonthAmort(CurSettings.MonitorAmortization);
        var totalDesignersRelatedCosts = designerSalary + oSLicensePricePerM + mainEadPricePerM +
                                         diskSpacePricePerM + thinClientPricePerM + monitorPricePerM;

        var totalCosts = totalCpuRelatedCosts + ramCostPerM + serverCostPerM + totalDesignersRelatedCosts;

        configStr +=
            $"   Опертивная память: {ramCostPerM:N1}{Environment.NewLine}" +
            $"   Интернет сервер: {serverCostPerM:N1}{Environment.NewLine}{Environment.NewLine}" +
            $"   Расходы зависящие от количества Проектировщиков:{Environment.NewLine}" +
            $"      Выплата зарплат: {designerSalary:N1}{Environment.NewLine}" +
            $"      Лицензии ОС: {oSLicensePricePerM:N1}{Environment.NewLine}" +
            $"      Лицензии Основной САПР: {mainEadPricePerM:N1}{Environment.NewLine}" + // ToDo
            $"      Дисковое Пространство: {diskSpacePricePerM:N1}{Environment.NewLine}" +
            $"      Тонкие клиенты: {thinClientPricePerM:N1}{Environment.NewLine}" +
            $"      Мониторы: {monitorPricePerM:N1} {Environment.NewLine}" +
            $"   ИТОГ: {totalDesignersRelatedCosts:N1}{Environment.NewLine}" +
            $"   {Environment.NewLine}" +
            $"СУММАРНЫЙ ИТОГ: {totalCosts:N1}";

        ExpenseReport = configStr;
    }

    public string ExpenseReport { get; private set; } = String.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;
}