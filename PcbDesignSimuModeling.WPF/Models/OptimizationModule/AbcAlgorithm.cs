using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Distributions;

namespace PcbDesignSimuModeling.WPF.Models.OptimizationModule;

public class AbcAlgorithm
{
    private readonly Random _rnd;

    private readonly Func<int, double, double, double, int, int, int, double> _objectiveFunction;
    private readonly AlgorithmSettings _algSettings;
    private readonly IntervalsOfVariables _intervals;
    private List<FoodSource> _foodSources;
    private double _curTemperature;

    public FoodSource BestFoodSource { get; private set; }


    public AbcAlgorithm(AlgorithmSettings algSettings, Random rnd,
        Func<int, double, double, double, int, int, int, double> objectiveFunction)
    {
        _algSettings = algSettings;
        _intervals = _algSettings.SearchIntervals;
        _rnd = rnd;
        _objectiveFunction = objectiveFunction;
        _curTemperature = _algSettings.InitTemperature;

        _foodSources = Enumerable.Range(0, _algSettings.FoodSourceCount)
            .Select(_ => GenerateNewFoodSource()).ToList();

        BestFoodSource = _foodSources.MaxBy(source => source.FuncValue)!;
    }

    private FoodSource GenerateNewFoodSource()
    {
        var x1 = DiscreteUniform.Sample(_rnd, _intervals.ThreadsCountMin, _intervals.ThreadsCountMax);
        var x2 = ContinuousUniform.Sample(_rnd, _intervals.ClockRateMin, _intervals.ClockRateMax);
        var x3 = ContinuousUniform.Sample(_rnd, _intervals.RamAmountMin, _intervals.RamAmountMax);
        var x4 = ContinuousUniform.Sample(_rnd, _intervals.ServerSpeedMin, _intervals.ServerSpeedMax);
        var x5 = _intervals.PlacingAlgsIndexes[0];
        var x6 = _intervals.WireRoutingAlgsIndexes[0];
        var x7 = DiscreteUniform.Sample(_rnd, _intervals.DesignersCountMin, _intervals.DesignersCountMax);
        return new FoodSource(x1, x2, x3, x4, x5, x6, x7, _objectiveFunction);
    }

    public IEnumerable<FoodSource> FindMinimum()
    {
        yield return BestFoodSource.Copy();

        for (var i = 0; i < _algSettings.NumOfIterations; i++)
        {
            //Поиск лучшего решения в окрестности текущего
            _foodSources = SearchNeighborsFoodSources();
            _foodSources.RemoveAll(source => source.NumberOfVisits >= _algSettings.FoodSourceCount);
            // if (!_foodSources.Contains(BestFoodSource))
            // {
            //     BestFoodSource.NumberOfVisits = 0;
            //     _foodSources.Add(BestFoodSource);
            // }
            
            
            //Генерируем скаутов, вместо источившихся источников еды
            var scoutsBeesCount = _algSettings.FoodSourceCount - _foodSources.Count;
            _foodSources.AddRange(Enumerable.Range(0, scoutsBeesCount).Select(_ => GenerateNewFoodSource()));

            //var best = _foodSources.MinBy(chromosome => chromosome.FuncValue).Copy();

            //Селекция
            var (j, selectedWorkers, selectedCount) = (0, new List<FoodSource>(), 0);
            while (selectedCount < _algSettings.FoodSourceCount)
            {
                var curWorker = _foodSources[j];
                var anotherWorker = _foodSources.FisherYatesShuffleExcept(curWorker, _rnd).First();

                if (curWorker.FuncValue >= anotherWorker.FuncValue ||
                    Math.Exp(-Math.Abs(curWorker.FuncValue - anotherWorker.FuncValue) / _curTemperature) >
                    _rnd.NextDouble())
                {
                    selectedWorkers.Add(curWorker);
                    selectedCount++;
                }

                j = (j + 1) % _algSettings.FoodSourceCount;
            }

            _foodSources = selectedWorkers;

            //Выбор лучшего и обновление температуры
            var iterBest = _foodSources.MaxBy(chromosome => chromosome.FuncValue)!;
            if (iterBest.FuncValue > BestFoodSource.FuncValue) BestFoodSource = iterBest;
            yield return BestFoodSource.Copy();

            _curTemperature = Math.Max(1.0,
                _algSettings.InitTemperature *
                Math.Pow(1.0 - i / (double) _algSettings.NumOfIterations, _algSettings.Alpha));
        }
    }

    private List<FoodSource> SearchNeighborsFoodSources()
    {
        var result = new List<FoodSource>();

        foreach (var curWorkerBee in _foodSources)
        {
            var dim = DiscreteUniform.Sample(_rnd, 1, 7);
            var neighborFoodSource = _foodSources.FisherYatesShuffleExcept(curWorkerBee, _rnd).First().Copy();

            switch (dim)
            {
                case 1:
                    neighborFoodSource.ThreadsCount = Math.Clamp(
                        Convert.ToInt32(
                            curWorkerBee.ThreadsCount +
                            ContinuousUniform.Sample(_rnd, -1.0, 1.0) *
                            (curWorkerBee.ThreadsCount - neighborFoodSource.ThreadsCount)),
                        _intervals.ThreadsCountMin, _intervals.ThreadsCountMax);
                    break;
                case 2:
                    neighborFoodSource.ClockRate = Math.Clamp(
                        curWorkerBee.ClockRate +
                        ContinuousUniform.Sample(_rnd, -1.0, 1.0) *
                        (curWorkerBee.ClockRate - neighborFoodSource.ClockRate),
                        _intervals.ClockRateMin, _intervals.ClockRateMax);
                    break;
                case 3:
                    neighborFoodSource.RamAmount = Math.Clamp(
                        curWorkerBee.RamAmount +
                        ContinuousUniform.Sample(_rnd, -1.0, 1.0) *
                        (curWorkerBee.RamAmount - neighborFoodSource.RamAmount),
                        _intervals.RamAmountMin, _intervals.RamAmountMax);
                    break;
                case 4:
                    neighborFoodSource.ServerSpeed = Math.Clamp(
                        curWorkerBee.ServerSpeed +
                        ContinuousUniform.Sample(_rnd, -1.0, 1.0) *
                        (curWorkerBee.ServerSpeed - neighborFoodSource.ServerSpeed),
                        _intervals.ServerSpeedMin, _intervals.ServerSpeedMax);
                    break;
                case 5:
                    neighborFoodSource.PlacingAlgIndex = 0;
                    break;
                case 6:
                    neighborFoodSource.WireRoutingAlgIndex = 0;
                    break;
                case 7:
                    neighborFoodSource.DesignersCount = Math.Clamp(
                        Convert.ToInt32(
                            curWorkerBee.DesignersCount +
                            ContinuousUniform.Sample(_rnd, -1.0, 1.0) *
                            (curWorkerBee.DesignersCount - neighborFoodSource.DesignersCount)),
                        _intervals.DesignersCountMin, _intervals.DesignersCountMax);
                    break;

                default: throw new ArgumentOutOfRangeException(nameof(dim));
            }

            neighborFoodSource.CalculateCost(_objectiveFunction);

            if (neighborFoodSource.FuncValue <= curWorkerBee.FuncValue)
            {
                curWorkerBee.NumberOfVisits++;
                result.Add(curWorkerBee);
            }
            else
            {
                neighborFoodSource.NumberOfVisits = 0;
                result.Add(neighborFoodSource);
            }
        }

        return result;
    }
}