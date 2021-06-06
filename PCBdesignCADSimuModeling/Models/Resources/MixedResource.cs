using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using PCBdesignCADSimuModeling.Annotations;

namespace PCBdesignCADSimuModeling.Models.Resources
{
    public abstract class MixedResource : IResource
    {
        protected virtual List<Guid> UtilizingProcIds { get; } = new();
        public abstract double ResValueForProc(Guid procId);
        public abstract void FreeResource(Guid procId);
        public abstract IResource Clone();
        public abstract double Cost { get; }
    }


    public class CpuThreads : MixedResource, INotifyPropertyChanged
    {
        protected override List<Guid> UtilizingProcIds => _procIdAndThread.Keys.ToList();
        private readonly int[] _threadAndTaskCount;
        private readonly Dictionary<Guid, List<int>> _procIdAndThread = new();
        private readonly double _oneThreadMultiTaskPenalty;
        private int _threadCount;
        private double _clockRate;


        public CpuThreads(int threadCount, double clockRate, double oneThreadMultiTaskPenalty = 0.9)
        {
            ThreadCount = threadCount;
            ClockRate = clockRate;
            _oneThreadMultiTaskPenalty = oneThreadMultiTaskPenalty;

            _threadAndTaskCount = Enumerable.Repeat(0, threadCount).ToArray();
        }


        public int ThreadCount
        {
            get => _threadCount;
            set
            {
                if (value == _threadCount) return;
                _threadCount = value;
                OnPropertyChanged(nameof(Cost));
            }
        }

        public double ClockRate
        {
            get => _clockRate;
            set
            {
                if (value.Equals(_clockRate)) return;
                _clockRate = value;
                OnPropertyChanged(nameof(Cost));
            }
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

        public override double ResValueForProc(Guid procId)
        {
            var curProcThreads = _procIdAndThread[procId];
            double threadSum = 0.0;

            foreach (var procThread in curProcThreads)
            {
                var taskCount = _threadAndTaskCount[procThread];
                threadSum += 1.0 / taskCount * Math.Pow(_oneThreadMultiTaskPenalty, taskCount - 1);
            }

            return threadSum * ClockRate;
        }

        public override void FreeResource(Guid procId)
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


        //


        public override IResource Clone() => new CpuThreads(this.ThreadCount, this.ClockRate);

        public override double Cost => Math.Round(
            (Math.Exp(ClockRate / 5.0) * 1.0 / ThreadCount +
             Math.Exp(ClockRate / 3.0) * (ThreadCount - 1.0) / ThreadCount) *
            Math.Pow(ThreadCount, 0.87)
            * 1000);

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}