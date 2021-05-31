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

    public abstract class MixedResources : Resource
    {
    }

    public abstract class SharedResources : Resource
    {
    }


    public class Designer : SharedResources
    {
        public ExperienceEn? Experience { get; init; }

        public enum ExperienceEn
        {
            Little,
            Average,
            Extensive
        }
    }

    public class Server : SharedResources
    {
        public double? InternetSpeed { get; init; }
    }

    public class CpuThread : MixedResources
    {
        public static List<CpuThread> CreateList(int count, double? clockRate = null) =>
            Enumerable.Range(1, count).Select(_ => new CpuThread() {ClockRate = clockRate}).ToList();

        public double? ClockRate { get; init; }
    }
}