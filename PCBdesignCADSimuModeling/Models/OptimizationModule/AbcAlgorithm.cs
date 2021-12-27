using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Distributions;

namespace PcbDesignCADSimuModeling.Models.OptimizationModule
{
    public class AbcAlgorithm
    {
        private readonly Random _rnd;

        private readonly Func<double, double, double, double, double, double, double> _objectiveFunction;
        private readonly AlgorithmSettings _algSettings;
        private readonly IntervalsOfVariables _intervals;
        private List<FoodSource> _foodSources = new();
        private double _curTemperature;

        public FoodSource BestFoodSource { get; private set; }


        public AbcAlgorithm(AlgorithmSettings algSettings, Random rnd,
            Func<double, double, double, double, double, double, double> objectiveFunction)
        {
            _algSettings = algSettings;
            _intervals = _algSettings.SearchIntervals;
            _rnd = rnd;
            _objectiveFunction = objectiveFunction;
            _curTemperature = _algSettings.InitTemperature;

            _foodSources = Enumerable.Range(0, _algSettings.PopulationSize)
                .Select(_ => GenerateNewFoodSource()).ToList();

            BestFoodSource = _foodSources.MinBy(source => source.Cost).Copy();
        }

        private FoodSource GenerateNewFoodSource()
        {
            var x1 = ContinuousUniform.Sample(_rnd, _intervals.ThreadsCountMin, _intervals.ThreadsCountMax);
            var x2 = ContinuousUniform.Sample(_rnd, _intervals.FreqMin, _intervals.FreqMax);
            var x3 = ContinuousUniform.Sample(_rnd, _intervals.ServerSpeedMin, _intervals.ServerSpeedMax);
            var x4 = ContinuousUniform.Sample(_rnd, _intervals.X4Low, _intervals.X4Up);
            var x5 = ContinuousUniform.Sample(_rnd, _intervals.X5Low, _intervals.X5Up);
            var x6 = ContinuousUniform.Sample(_rnd, _intervals.DesignersCountMin, _intervals.DesignersCountMax);
            return new FoodSource(x1, x2, x3, x4, x5, x6, _objectiveFunction);
        }

        public IEnumerable<FoodSource> FindMinimum()
        {
            yield return BestFoodSource.Copy();

            for (var i = 0; i < _algSettings.NumOfIterations; i++)
            {
                //Поиск лучшего решения в окрестности текущего
                _foodSources = SearchNeighborsFoodSources();
                _foodSources.RemoveAll(source =>
                    source.CurNumOfUses >= _algSettings.PopulationSize && source != BestFoodSource);

                //Генерируем скаутов, вместо источившихся источников еды
                var scoutsBeesCount = _algSettings.PopulationSize - _foodSources.Count;
                _foodSources.AddRange(Enumerable.Range(0, scoutsBeesCount).Select(_ => GenerateNewFoodSource()));

                //Селекция
                var (ii, selectedWorkers, selectedCount) = (0, new List<FoodSource>(), 0);
                while (selectedCount < _algSettings.PopulationSize)
                {
                    var curWorker = _foodSources[ii];
                    var anotherWorker = _foodSources.FisherYatesShuffleExcept(curWorker, _rnd).First();

                    if (curWorker.Cost <= anotherWorker.Cost ||
                        Math.Exp(-Math.Abs(curWorker.Cost - anotherWorker.Cost) / _curTemperature) > _rnd.NextDouble())
                    {
                        selectedWorkers.Add(curWorker);
                        selectedCount++;
                    }

                    ii = (ii + 1) % _algSettings.PopulationSize;
                }

                _foodSources = selectedWorkers;

                //Выбор лучшего и обновление температуры
                var iterBest = _foodSources.MinBy(chromosome => chromosome.Cost).Copy();
                if (iterBest.Cost < BestFoodSource.Cost) BestFoodSource = iterBest;
                yield return BestFoodSource.Copy();

                _curTemperature = Math.Max(1e-4,
                    _algSettings.InitTemperature *
                    Math.Pow(1.0 - i / (double)_algSettings.NumOfIterations, _algSettings.Alpha));
            }
        }

        private List<FoodSource> SearchNeighborsFoodSources()
        {
            var result = new List<FoodSource>();

            foreach (var curWorkerBee in _foodSources)
            {
                var dim = _rnd.Next(1, 6 + 1);
                var neighborFoodSource = _foodSources.FisherYatesShuffleExcept(curWorkerBee, _rnd).First().Copy();

                switch (dim)
                {
                    case 1:
                        neighborFoodSource.X1 =
                            Math.Clamp(
                                curWorkerBee.X1 + (-1.0 + _rnd.NextDouble() * 2.0) *
                                (curWorkerBee.X1 - neighborFoodSource.X1),
                                _intervals.ThreadsCountMin, _intervals.ThreadsCountMax);
                        break;
                    case 2:
                        neighborFoodSource.X2 =
                            Math.Clamp(
                                curWorkerBee.X2 + (-1.0 + _rnd.NextDouble() * 2.0) *
                                (curWorkerBee.X2 - neighborFoodSource.X2),
                                _intervals.FreqMin, _intervals.FreqMax);
                        break;
                    case 3:
                        neighborFoodSource.X3 =
                            Math.Clamp(
                                curWorkerBee.X3 + (-1.0 + _rnd.NextDouble() * 2.0) *
                                (curWorkerBee.X3 - neighborFoodSource.X3),
                                _intervals.ServerSpeedMin, _intervals.ServerSpeedMax);
                        break;
                    case 4:
                        neighborFoodSource.X4 =
                            Math.Clamp(
                                curWorkerBee.X4 + (-1.0 + _rnd.NextDouble() * 2.0) *
                                (curWorkerBee.X4 - neighborFoodSource.X4),
                                _intervals.X4Low, _intervals.X4Up);
                        break;
                    case 5:
                        neighborFoodSource.X5 =
                            Math.Clamp(
                                curWorkerBee.X5 + (-1.0 + _rnd.NextDouble() * 2.0) *
                                (curWorkerBee.X5 - neighborFoodSource.X5),
                                _intervals.X5Low, _intervals.X5Up);
                        break;
                    case 6:
                        neighborFoodSource.X6 =
                            Math.Clamp(
                                curWorkerBee.X6 + (-1.0 + _rnd.NextDouble() * 2.0) *
                                (curWorkerBee.X6 - neighborFoodSource.X6),
                                _intervals.DesignersCountMin, _intervals.DesignersCountMax);
                        break;

                    default: throw new ArgumentOutOfRangeException(nameof(dim));
                }

                neighborFoodSource.CalculateCost(_objectiveFunction);

                if (neighborFoodSource.Cost >= curWorkerBee.Cost)
                {
                    curWorkerBee.CurNumOfUses++;
                    result.Add(curWorkerBee);
                }
                else
                {
                    neighborFoodSource.CurNumOfUses = 0;
                    result.Add(neighborFoodSource);
                }
            }

            return result;
        }
    }
}