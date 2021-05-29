using System;
using System.Collections.Generic;
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
    
    
    

    


    
    
    public static class PlacingAlgProviderFactory
    {
        private static readonly Dictionary<string, Func<IPlacingAlgFactory>> Map = new();

        static PlacingAlgProviderFactory()
        {
            Map["Example"] = () => 
                new PlacingAlgFactory(pcbParams => 
                    new PlacingMultiThreadAlgorithm(new PlacingExampleCxtyEst(pcbParams), 8, 0.7));
        }

        public static IPlacingAlgFactory Create(string placingAlgName)
        {
            var creator = 
                GetCreator(placingAlgName) ?? throw new UnsupportedPcbAlgException(placingAlgName);

            return creator();
        }
        
        private static Func<IPlacingAlgFactory> GetCreator(string placingAlgName)
        {
            Map.TryGetValue(placingAlgName, out var creator);
            return creator;
        }
    }
    public static class WireRoutingAlgProviderFactory
    {
        private static readonly Dictionary<string, Func<IWireRoutingAlgFactory>> Map = new();

        static WireRoutingAlgProviderFactory()
        {
            // _placingMap["Example"] = pcbParams =>
            //     new PlacingMultiThreadAlgorithm(new PlacingExampleCxtyEst(pcbParams), 8, 0.7);
            Map["Example"] = () => 
                new WireRoutingAlgFactory(pcbParams => 
                    new WireRoutingMultiThreadAlgorithm(new WireRoutingExampleCxtyEst(pcbParams), 8, 0.7));
        }

        public static IWireRoutingAlgFactory Create(string wireRoutingAlgName)
        {
            var creator = 
                GetCreator(wireRoutingAlgName) ?? throw new UnsupportedPcbAlgException(wireRoutingAlgName);

            return creator();
        }
        
        private static Func<IWireRoutingAlgFactory> GetCreator(string wireRoutingAlgName)
        {
            Map.TryGetValue(wireRoutingAlgName, out var creator);
            return creator;
        }
    }
    
    


    public interface IPlacingAlgFactory
    {
        public IPlacingAlgorithm Create(PcbParams pcbInfo);
    }
    public interface IWireRoutingAlgFactory
    {
        public IWireRoutingAlgorithm Create(PcbParams pcbInfo);
    }

    public class PlacingAlgFactory : IPlacingAlgFactory
    {
        private readonly Func<PcbParams, IPlacingAlgorithm> _funcPlacing;

        public PlacingAlgFactory(Func<PcbParams, IPlacingAlgorithm> funcPlacing)
        {
            _funcPlacing = funcPlacing;
        }

        public IPlacingAlgorithm Create(PcbParams pcbInfo)
        {
            return _funcPlacing(pcbInfo);
        }
    }
    public class WireRoutingAlgFactory : IWireRoutingAlgFactory
    {
        private readonly Func<PcbParams, IWireRoutingAlgorithm> _funcWireRouting;

        public WireRoutingAlgFactory(Func<PcbParams, IWireRoutingAlgorithm> funcWireRouting)
        {
            _funcWireRouting = funcWireRouting;
        }

        public IWireRoutingAlgorithm Create(PcbParams pcbInfo)
        {
            return _funcWireRouting(pcbInfo);
        }
    }

    public class PcbAlgFactories
    {
        public IPlacingAlgFactory PlacingAlgFactory { get; init; }
        public IWireRoutingAlgFactory WireRoutingAlgFactory { get; init; }
    }

    
    public class UnsupportedPcbAlgException : Exception
    {
        public UnsupportedPcbAlgException(string wireRoutingAlgName) : base($"Algorithm: {wireRoutingAlgName} is not Supported")
        {
        }
    }
}