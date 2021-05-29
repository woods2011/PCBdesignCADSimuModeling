using System;
using PCBdesignCADSimuModeling.Models.Resources.Algorithms.PlacingAlgorithms;
using PCBdesignCADSimuModeling.Models.Resources.Algorithms.WireRoutingAlgorithms;
using PCBdesignCADSimuModeling.Models.Technologies.PcbDesign;

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



    
    

    public interface IPcbAlgorithmFactoryHoldPcbInfo
    {
        IWireRoutingAlgorithm WireRoutingAlgorithmInstance();
        IPlacingAlgorithm PlacingAlgorithmInstance();
    }

    class PcbAlgorithmFactoryHoldPcbInfo<TPcbInfo> : IPcbAlgorithmFactoryHoldPcbInfo
    {
        private readonly TPcbInfo _pcbInfo;
        private readonly IPcbAlgorithmFactory<TPcbInfo> _pcbAlgorithmFactory;
        
        public PcbAlgorithmFactoryHoldPcbInfo(TPcbInfo pcbInfo, IPcbAlgorithmFactory<TPcbInfo> pcbAlgorithmFactory)
        {
            _pcbInfo = pcbInfo;
            _pcbAlgorithmFactory = pcbAlgorithmFactory;
        }

        public IWireRoutingAlgorithm WireRoutingAlgorithmInstance()
        {
            return _pcbAlgorithmFactory.WireRoutingAlgorithmInstance(_pcbInfo);
        }

        public IPlacingAlgorithm PlacingAlgorithmInstance()
        {
            return _pcbAlgorithmFactory.PlacingAlgorithmInstance(_pcbInfo);
        }
    }

    public interface IPcbAlgorithmFactory<TPcbInfo>
    {
        public IWireRoutingAlgorithm WireRoutingAlgorithmInstance(TPcbInfo pcbInfo);

        public IPlacingAlgorithm PlacingAlgorithmInstance(TPcbInfo pcbInfo);
    }

    public class PcbAlgorithmFactory<TPcbInfo> : IPcbAlgorithmFactory<TPcbInfo>
    {
        private readonly Func<TPcbInfo, IPlacingAlgorithm> _funcPlacing;
        private readonly Func<TPcbInfo, IWireRoutingAlgorithm> _funcWireRouting;

        public PcbAlgorithmFactory(Func<TPcbInfo, IWireRoutingAlgorithm> funcWireRouting, Func<TPcbInfo, IPlacingAlgorithm> funcPlacing)
        {
            _funcWireRouting = funcWireRouting;
            _funcPlacing = funcPlacing;
        }

        public IWireRoutingAlgorithm WireRoutingAlgorithmInstance(TPcbInfo pcbInfo)
        {
            return _funcWireRouting(pcbInfo);
        }

        public IPlacingAlgorithm PlacingAlgorithmInstance(TPcbInfo pcbInfo)
        {
            return _funcPlacing(pcbInfo);
        }
    }
}