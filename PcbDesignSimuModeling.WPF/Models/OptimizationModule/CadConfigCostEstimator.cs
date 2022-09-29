using System;
using System.Collections.Generic;
using System.Linq;
using PcbDesignSimuModeling.WPF.Models.Resources;
using PcbDesignSimuModeling.WPF.Models.Resources.CpuClusterRes;
using PcbDesignSimuModeling.WPF.Models.Resources.DesignerRes;
using PcbDesignSimuModeling.WPF.Models.Resources.RamRes;
using PcbDesignSimuModeling.WPF.Models.Resources.ServerRes;
using static PcbDesignSimuModeling.WPF.Models.SimuSystem.GeneralSimulationSettings;

namespace PcbDesignSimuModeling.WPF.Models.OptimizationModule;

public class CadConfigCostEstimator
{
    public decimal EstimateFullCost(IEnumerable<IResource> resources)
    {
        var resourcesList = resources.ToList();

        var cpuCluster = resourcesList.OfType<CpuCluster>().First();
        var ram = resourcesList.OfType<Ram>().First();
        var server = resourcesList.OfType<Server>().First();
        var designersCount = resourcesList.OfType<Designer>().Count();

        if (cpuCluster.CoveringConfig is null) return Decimal.MaxValue;
        var (cpuModel, isDualSocket) = cpuCluster.CoveringConfig.Value;

        var cpuCostPerM = cpuCluster.CostPerMonth;
        var twoSocMBoardPricePerM = CurSettings.DualSocketMBoardPrice.WithMonthAmort(CurSettings.MBoardAmortization);
        var oneSocMBoardPricePerM = CurSettings.OneSocketMBoardPrice.WithMonthAmort(CurSettings.MBoardAmortization);
        var virtualizationPricePerM =
            cpuModel.ThreadCount / 2m *
            CurSettings.VirtualizationLicensePerCorePrice.WithMonthAmort(CurSettings.VirtualizationAmortization);
        var totalCpuRelatedCosts = (isDualSocket ? twoSocMBoardPricePerM : oneSocMBoardPricePerM) +
                                   cpuCostPerM +
                                   virtualizationPricePerM;

        var ramCostPerM = ram.CostPerMonth;
        var serverCostPerM = server.CostPerMonth;

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

        return totalCosts;
    }
}