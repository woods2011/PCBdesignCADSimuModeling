using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PCBdesignCADSimuModeling.Models.Resources
{
    public abstract class Resource
    {
    }

    public abstract class UndividedResource : Resource
    {
    }

    public abstract class MixedResource : Resource
    {
    }

    public abstract class SharedResource : Resource
    {
    }


    public class Designer : SharedResource
    {
        public ExperienceEn? Experience { get; init; }

        public enum ExperienceEn
        {
            Little,
            Average,
            Extensive
        }
    }

    public class Server : SharedResource
    {
        public double? InternetSpeed { get; init; }
    }
    
    // public class CpuThreads : MixedResources
    // {
    //     public int Count { get; init; } = 1;
    //
    //     public double? ClockRate { get; init; }
    // }

    public class CpuThreads : MixedResource
    {
        public static List<CpuThreads> CreateList(int count, double? clockRate = null) =>
            Enumerable.Range(1, count).Select(_ => new CpuThreads() {ClockRate = clockRate}).ToList();
    
        public double? ClockRate { get; init; }
    }
}