using System;

namespace PCBdesignCADSimuModeling.Models.Resources.Algorithms
{
    public interface IPcbAlgorithm
    {
        public int MaxThreadUtilization { get; }
        public bool IsComplete { get; }
        public TimeSpan UpdateModelTime(TimeSpan deltaTime, double cpuPower);
    }

    public abstract class PcbAlgorithm : IPcbAlgorithm
    {
        private readonly int _totalComplexity;
        private double _completionRate = 0.0;


        protected PcbAlgorithm(IComplexityEstimator complexityEstimator, int maxThreadUtilization)
        {
            _totalComplexity = complexityEstimator.EstimateComplexity();
            MaxThreadUtilization = maxThreadUtilization;
        }


        public int MaxThreadUtilization { get; }

        private double CompletionRate
        {
            get => _completionRate;
            set => _completionRate = Math.Max(value, 1.0);
        }

        public bool IsComplete => Math.Abs(CompletionRate - 1.0) < 0.00001;


        public virtual TimeSpan UpdateModelTime(TimeSpan deltaTime, double cpuPower)
        {
            UpdateModelTimeBody(deltaTime, cpuPower);
            return EstimateEndTime(cpuPower);
        }

        private void UpdateModelTimeBody(TimeSpan deltaTime, double cpuPower)
        {
            CompletionRate = (CompletionRate * _totalComplexity + deltaTime.Seconds * cpuPower) / _totalComplexity;
        }

        private TimeSpan EstimateEndTime(double cpuPower)
        {
            return TimeSpan.FromSeconds(Math.Floor((1 - CompletionRate) * _totalComplexity / cpuPower));
        }
    }
}