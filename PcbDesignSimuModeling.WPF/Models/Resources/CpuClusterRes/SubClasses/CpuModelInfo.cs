using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PcbDesignSimuModeling.WPF.Models.Resources.CpuClusterRes.SubClasses;

public class CpuModelInfo
{
    public double ClockRate { get; }
    public int ThreadCount { get; }
    public decimal Price { get; }
    public bool DualProcessing { get; }
    public string Name { get; }


    private CpuModelInfo(int threadCount, double clockRate, decimal price, bool dualProcessing = true,
        [CallerMemberName] string? name = null)
    {
        ThreadCount = threadCount;
        ClockRate = clockRate;
        Price = price;
        Name = name is not null ? $"Ryzen {name.ToUpper().Replace("EPYC", "EPYC ")}" : String.Empty;
        DualProcessing = dualProcessing;
    }

    public static (CpuModelInfo cpuModel, bool IsDualSocket)? ChooseConfigurationOrDefault(
        int threadCount, double clockRate, (decimal OneSoc, decimal TwoSoc) mBoardPrices)
    {
        var rateFilteredList = CpuModelsList.Where(info => info.ClockRate >= clockRate - 1e-12).ToList();
        if (rateFilteredList.Count == 0) return null;

        var oneSocContender = rateFilteredList
            .Where(info => info.ThreadCount * (1 + 0.9 * (info.ClockRate / clockRate - 1)) >= threadCount)
            .MinBy(info => info.Price);
        var twoSocContender = rateFilteredList
            .Where(info => info.DualProcessing)
            .Where(info => info.ThreadCount < threadCount)
            .Where(info => info.ThreadCount * 2 * (1 + 0.9 * (info.ClockRate / clockRate - 1)) >= threadCount)
            .MinBy(info => info.Price);
        // var oneSocContender = rateFilteredList.Where(info => info.ThreadCount >= threadCount).MinBy(info => info.Price);
        // var twoSocContender = rateFilteredList
        //     .Where(info => info.DualProcessing)
        //     .Where(info => info.ThreadCount < threadCount)
        //     .Where(info => info.ThreadCount * 2 >= threadCount)
        //     .MinBy(info => info.Price);

        if (oneSocContender is not null && twoSocContender is null)
            return (oneSocContender, false);

        if (oneSocContender is null && twoSocContender is not null)
            return (twoSocContender, true);

        if (oneSocContender is null || twoSocContender is null) return null;

        var oneSocContenderBuildPrice = oneSocContender.Price + mBoardPrices.OneSoc;
        var twoSocContenderBuildPrice = twoSocContender.Price * 2 + mBoardPrices.TwoSoc;
        var twoSocPenalty = Convert.ToDecimal(1.3 * oneSocContender.ClockRate / twoSocContender.ClockRate);
        return oneSocContenderBuildPrice < twoSocContenderBuildPrice * twoSocPenalty
            ? (oneSocContender, false)
            : (twoSocContender, true);
    }

    public static CpuModelInfo Epyc72F3 => new(16, 4.1, 2468 * 79);
    public static CpuModelInfo Epyc73F3 => new(32, 3.9, 3521 * 79);
    public static CpuModelInfo Epyc74F3 => new(48, 3.8, 2900 * 79);
    public static CpuModelInfo Epyc75F3 => new(64, 3.8, 4860 * 79);

    public static CpuModelInfo Epyc7713 => new(128, 3.2, 7060 * 79);
    public static CpuModelInfo Epyc7713P => new(128, 3.2, 5010 * 79, false);
    public static CpuModelInfo Epyc7663 => new(112, 3.0, 6366 * 79);
    public static CpuModelInfo Epyc7643 => new(96, 3.35, 4995 * 79);

    public static CpuModelInfo Epyc7543 => new(64, 3.45, 3761 * 79);

    public static CpuModelInfo Epyc7543P => new(64, 3.45, 2730 * 79, false);

    //public static CpuModelInfo Epyc7513 => new(64, 3.35, 2840 * 79);
    //public static CpuModelInfo Epyc7453 => new(56, 3.4, 1570 * 79);

    public static CpuModelInfo Epyc7443 => new(48, 3.6, 2010 * 79);
    public static CpuModelInfo Epyc7443P => new(48, 3.6, 1337 * 79, false);
    public static CpuModelInfo Epyc7413 => new(48, 3.35, 1825 * 79);
    public static CpuModelInfo Epyc7343 => new(32, 3.7, 1565 * 79);
    public static CpuModelInfo Epyc7313 => new(32, 3.45, 1083 * 79);
    public static CpuModelInfo Epyc7313P => new(32, 3.45, 913 * 79, false);

    public static IReadOnlyList<CpuModelInfo> CpuModelsList { get; } = new List<CpuModelInfo>()
    {
        Epyc72F3,
        Epyc73F3,
        Epyc74F3,
        Epyc75F3,
        Epyc7713,
        Epyc7713P,
        Epyc7663,
        Epyc7643,
        Epyc7543,
        Epyc7543P,
        //Epyc7513,
        //Epyc7453,
        Epyc7443,
        Epyc7443P,
        Epyc7413,
        Epyc7343,
        Epyc7313,
        Epyc7313P
    };

    // public static CpuModelInfo Epyc72F3 => new CpuModelInfo(16, 4.0, 2468);
    // public static CpuModelInfo Epyc73F3 => new CpuModelInfo(32, 3.9, 3521);
    // public static CpuModelInfo Epyc74F3 => new CpuModelInfo(48, 3.8, 2900);
    // public static CpuModelInfo Epyc75F3 => new CpuModelInfo(64, 3.8, 4860);
    //
    // public static CpuModelInfo Epyc7713 => new CpuModelInfo(128, 3.2, 7060);
    // public static CpuModelInfo Epyc7713P => new CpuModelInfo(128, 3.2, 5010, false);
    // public static CpuModelInfo Epyc7663 => new CpuModelInfo(112, 3.0, 6366);
    // public static CpuModelInfo Epyc7643 => new CpuModelInfo(96, 3.35, 4995);
    //
    // public static CpuModelInfo Epyc7543 => new CpuModelInfo(64, 3.45, 3761);
    // public static CpuModelInfo Epyc7543P => new CpuModelInfo(64, 3.45, 2730, false);
    // public static CpuModelInfo Epyc7513 => new CpuModelInfo(64, 3.35, 2840);
    // public static CpuModelInfo Epyc7453 => new CpuModelInfo(56, 3.4, 1570);
    // public static CpuModelInfo Epyc7443 => new CpuModelInfo(48, 3.6, 2010);
    // public static CpuModelInfo Epyc7443P => new CpuModelInfo(48, 3.6, 1337, false);
    // public static CpuModelInfo Epyc7413 => new CpuModelInfo(48, 3.35, 1825);
    // public static CpuModelInfo Epyc7343 => new CpuModelInfo(32, 3.7, 1565);
    // public static CpuModelInfo Epyc7313 => new CpuModelInfo(32, 3.45, 1083);
    // public static CpuModelInfo Epyc7313P => new CpuModelInfo(32, 3.45, 913, false);
}