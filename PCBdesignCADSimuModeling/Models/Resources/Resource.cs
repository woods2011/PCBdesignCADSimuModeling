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
        public abstract double ResValueForProc(Guid procId);
    }

    public abstract class SharedResource : Resource
    {
    }

    public class Designer : UndividedResource
    {
        public Designer(ExperienceEn experience)
        {
            Experience = experience;
        }

        public ExperienceEn Experience { get; }

        public enum ExperienceEn
        {
            Little,
            Average,
            Extensive
        }
    }


    public class Server : SharedResource
    {
        public Server(double internetSpeed)
        {
            InternetSpeed = internetSpeed;
        }

        public double InternetSpeed { get; }
    }

    public class CpuThreads : MixedResource
    {
        private readonly int[] _threadAndTaskCount;
        private readonly Dictionary<Guid, List<int>> _procIdAndThread = new();
        private const double OneThreadMultiTaskPenalty = 0.95;

        public CpuThreads(int threadCount, double clockRate)
        {
            ThreadCount = threadCount;
            ClockRate = clockRate;
            _threadAndTaskCount = Enumerable.Repeat(0, threadCount).ToArray();
        }


        public int ThreadCount { get; }
        public double ClockRate { get; }

        public override double ResValueForProc(Guid procId)
        {
            var curProcThreads = _procIdAndThread[procId];
            double threadSum = 0.0;

            foreach (var procThread in curProcThreads)
            {
                var taskCount = _threadAndTaskCount[procThread];
                threadSum += 1.0 / taskCount * Math.Pow(OneThreadMultiTaskPenalty, taskCount - 1);
            }

            return threadSum * ClockRate;
        }

        public bool TryGetResource(Guid procId, int reqThreadCount)
        {
            var threadsList = new List<int>();

            for (var i = 0; i < reqThreadCount; i++)
            {
                var indexOfOptimalThread = Array.IndexOf(_threadAndTaskCount, _threadAndTaskCount.Min(num => num));
                _threadAndTaskCount[indexOfOptimalThread]++;
                threadsList.Add(indexOfOptimalThread);
            }

            _procIdAndThread.Add(procId, threadsList);

            return true;
        }

        public void FreeResource(Guid procId)
        {
            var threadsList = _procIdAndThread[procId];

            foreach (var indexOfThread in threadsList)
            {
                _threadAndTaskCount[indexOfThread]--;
            }

            _procIdAndThread.Remove(procId);
            
            BalanceRes();
        }

        protected virtual void BalanceRes()
        {
            var maxLoad = _threadAndTaskCount.Max(i1 => i1);
            var minLoad = _threadAndTaskCount.Min(i1 => i1);
            var realDeltaLoad = maxLoad - minLoad - 1;

            while (realDeltaLoad > 0)
            {
                var indexOfMaxLoadedThread = Array.IndexOf(_threadAndTaskCount, maxLoad);
                var indexOfMinLoadedThread = Array.IndexOf(_threadAndTaskCount, minLoad);
                _threadAndTaskCount[indexOfMaxLoadedThread] -= realDeltaLoad;
                _threadAndTaskCount[indexOfMinLoadedThread] += realDeltaLoad;

                for (var i = 0; i < realDeltaLoad; i++)
                {
                    // Procedures that take less threads are transferred first
                    _procIdAndThread.Values
                        .OrderBy(threadList => threadList.Count)
                        .FirstOrDefault(threadsOfProc => threadsOfProc.Remove(indexOfMaxLoadedThread))
                        ?.Add(indexOfMinLoadedThread);
                }

                maxLoad = _threadAndTaskCount.Max(i1 => i1);
                minLoad = _threadAndTaskCount.Min(i1 => i1);
                realDeltaLoad = maxLoad - minLoad - 1;
            }
        }
    }
}