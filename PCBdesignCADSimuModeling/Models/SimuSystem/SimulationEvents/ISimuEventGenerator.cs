using System;
using System.Collections.Generic;
using MathNet.Numerics.Distributions;
using PCBdesignCADSimuModeling.Models.Technologies.PcbDesign;

namespace PCBdesignCADSimuModeling.Models.SimuSystem.SimulationEvents
{
    public interface ISimuEventGenerator
    {
        List<SimulationEvent> GeneratePcbDesignTech(TimeSpan finalTime, TimeSpan? startTime = null);
    }


    public class SimuEventGenerator : ISimuEventGenerator
    {
        private IContinuousDistribution _pcbDesignTechIntervalDistr;
        private IContinuousDistribution _elementCountDistr, _dimensionUsagePctDistr, _variousSizePctDistr;


        private SimuEventGenerator()
        {
        }


        public List<SimulationEvent> GeneratePcbDesignTech(TimeSpan finalTime, TimeSpan? startTime = null)
        {
            List<SimulationEvent> simulationEvents = new();
            var curTime = startTime ?? TimeSpan.Zero;

            curTime += TimeSpan.FromSeconds(
                Math.Round(Math.Max(0.0, _pcbDesignTechIntervalDistr.Sample())));

            while (curTime < finalTime)
            {
                simulationEvents.Add(new PcbDesignTechnologyStart(curTime, GeneratePcbParams()));

                curTime += TimeSpan.FromSeconds(
                    Math.Round(Math.Max(0.0, _pcbDesignTechIntervalDistr.Sample())));
            }

            return simulationEvents;
        }

        private PcbParams GeneratePcbParams()
        {
            return new PcbParams((int) Math.Round(Math.Max(0.0, _elementCountDistr.Sample())),
                _dimensionUsagePctDistr.Sample(), _variousSizePctDistr.Sample() >= 0.5);
        }


        public class Builder
        {
            private readonly SimuEventGenerator _eventGenerator = new SimuEventGenerator();

            public Builder()
            {
            }

            public SimuEventGeneratorBuilderStep2 NewTechInterval(
                IContinuousDistribution pcbDesignTechIntervalDistr)
            {
                _eventGenerator._pcbDesignTechIntervalDistr = pcbDesignTechIntervalDistr;
                return new SimuEventGeneratorBuilderStep2(_eventGenerator);
            }
        }

        public class SimuEventGeneratorBuilderStep2
        {
            private readonly SimuEventGenerator _eventGenerator;

            public SimuEventGeneratorBuilderStep2(SimuEventGenerator eventGenerator)
            {
                _eventGenerator = eventGenerator;
            }

            public SimuEventGeneratorBuilderFinal PcbParams(IContinuousDistribution elementCountDistr,
                IContinuousDistribution dimensionUsagePctDistr, IContinuousDistribution variousSizePctDistr)
            {
                _eventGenerator._elementCountDistr = elementCountDistr;
                _eventGenerator._dimensionUsagePctDistr = dimensionUsagePctDistr;
                _eventGenerator._variousSizePctDistr = variousSizePctDistr;
                return new SimuEventGeneratorBuilderFinal(_eventGenerator);
            }
        }

        public class SimuEventGeneratorBuilderFinal
        {
            private readonly SimuEventGenerator _eventGenerator;

            public SimuEventGeneratorBuilderFinal(SimuEventGenerator eventGenerator)
            {
                _eventGenerator = eventGenerator;
            }

            public SimuEventGenerator Build() => _eventGenerator;
        }
    }
}