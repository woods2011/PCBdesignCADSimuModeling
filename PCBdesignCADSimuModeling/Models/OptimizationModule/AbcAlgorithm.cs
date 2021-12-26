using System;
using System.Collections.Generic;
using System.Linq;

namespace PcbDesignCADSimuModeling.Models.OptimizationModule
{
    public class AbcAlgorithm
    {
        private readonly Random _rnd;

        private readonly Func<double, double, double, double, double, double, double> _objectiveFunction;
        private readonly AlgorithmParameters _algorithmParams;
        private readonly IntervalsOfVariables _intervals;
        private List<FoodSource> _foodSources = new();
        private double _curTemperature;

        public FoodSource BestFoodSource { get; private set; }


        public AbcAlgorithm(AlgorithmParameters algorithmParams, Random rnd,
            Func<double, double, double, double, double, double, double> objectiveFunction)
        {
            _algorithmParams = algorithmParams;
            _intervals = _algorithmParams.SearchIntervals;
            _rnd = rnd;
            _objectiveFunction = objectiveFunction;
            _curTemperature = _algorithmParams.InitTemperature;

            for (var i = 0; i < algorithmParams.PopulationSize; i++)
            {
                var x1 = _intervals.X1Low + _rnd.NextDouble() * (_intervals.X1Up - _intervals.X1Low);
                var x2 = _intervals.X2Low + _rnd.NextDouble() * (_intervals.X2Up - _intervals.X2Low);
                var x3 = _intervals.X3Low + _rnd.NextDouble() * (_intervals.X3Up - _intervals.X3Low);
                var x4 = _intervals.X4Low + _rnd.NextDouble() * (_intervals.X4Up - _intervals.X4Low);
                var x5 = _intervals.X5Low + _rnd.NextDouble() * (_intervals.X5Up - _intervals.X5Low);
                var x6 = _intervals.X6Low + _rnd.NextDouble() * (_intervals.X6Up - _intervals.X6Low);
                _foodSources.Add(new FoodSource(x1, x2, x3, x4, x5, x6, _objectiveFunction));
            }

            BestFoodSource = _foodSources.MinBy(source => source.Cost).Copy();
        }

        public IEnumerable<FoodSource> FindMinimum()
        {
            yield return BestFoodSource.Copy();

            for (var i = 0; i < _algorithmParams.NumOfIterations; i++)
            {
                //Поиск лучшего решения в окрестности текущего
                _foodSources = TryNeighborsFoodSources();
                _foodSources.RemoveAll(source =>
                    source.CurNumOfUses >= _algorithmParams.PopulationSize && source != BestFoodSource);

                //Генерируем скаутов, вместо источившихся источников еды
                var scoutsBeesCount = _algorithmParams.PopulationSize - _foodSources.Count;
                for (var j = 0; j < scoutsBeesCount; j++)
                {
                    var x1 = _intervals.X1Low + _rnd.NextDouble() * (_intervals.X1Up - _intervals.X1Low);
                    var x2 = _intervals.X2Low + _rnd.NextDouble() * (_intervals.X2Up - _intervals.X2Low);
                    var x3 = _intervals.X3Low + _rnd.NextDouble() * (_intervals.X3Up - _intervals.X3Low);
                    var x4 = _intervals.X4Low + _rnd.NextDouble() * (_intervals.X4Up - _intervals.X4Low);
                    var x5 = _intervals.X5Low + _rnd.NextDouble() * (_intervals.X5Up - _intervals.X5Low);
                    var x6 = _intervals.X6Low + _rnd.NextDouble() * (_intervals.X6Up - _intervals.X6Low);
                    _foodSources.Add(new FoodSource(x1, x2, x3, x4, x5, x6, _objectiveFunction));
                }

                //Селекция
                var selected = 0;
                var z = 0;
                var selectedWorkers = new List<FoodSource>();
                while (selected < _algorithmParams.PopulationSize)
                {
                    var curWorker = _foodSources[z];
                    var anotherWorker = _foodSources.FisherYatesShuffleExcept(curWorker, _rnd).First();

                    if (curWorker.Cost <= anotherWorker.Cost ||
                        Math.Exp(-Math.Abs(curWorker.Cost - anotherWorker.Cost) / _curTemperature) > _rnd.NextDouble())
                    {
                        selectedWorkers.Add(curWorker);
                        selected++;
                    }

                    z = (z + 1) % _algorithmParams.PopulationSize;
                }

                _foodSources = selectedWorkers;

                //Выбор лучшего и обновление температуры
                var iterBest = _foodSources.MinBy(chromosome => chromosome.Cost).Copy();
                if (iterBest.Cost < BestFoodSource.Cost) BestFoodSource = iterBest;
                yield return BestFoodSource.Copy();

                _curTemperature = Math.Max(_algorithmParams.MinimalTemperature,
                    _algorithmParams.InitTemperature *
                    Math.Pow(1.0 - i / (double)_algorithmParams.NumOfIterations, _algorithmParams.Alpha));
            }
        }

        private List<FoodSource> TryNeighborsFoodSources()
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
                                curWorkerBee.X1 + (-1.0 + _rnd.NextDouble() * 2.0) * (curWorkerBee.X1 - neighborFoodSource.X1),
                                _intervals.X1Low, _intervals.X1Up);
                        break;
                    case 2:
                        neighborFoodSource.X2 =
                            Math.Clamp(
                                curWorkerBee.X2 + (-1.0 + _rnd.NextDouble() * 2.0) * (curWorkerBee.X2 - neighborFoodSource.X2),
                                _intervals.X2Low, _intervals.X2Up);
                        break;
                    case 3:
                        neighborFoodSource.X3 =
                            Math.Clamp(
                                curWorkerBee.X3 + (-1.0 + _rnd.NextDouble() * 2.0) * (curWorkerBee.X3 - neighborFoodSource.X3),
                                _intervals.X3Low, _intervals.X3Up);
                        break;
                    case 4:
                        neighborFoodSource.X4 =
                            Math.Clamp(
                                curWorkerBee.X4 + (-1.0 + _rnd.NextDouble() * 2.0) * (curWorkerBee.X4 - neighborFoodSource.X4),
                                _intervals.X4Low, _intervals.X4Up);
                        break;
                    case 5:
                        neighborFoodSource.X5 =
                            Math.Clamp(
                                curWorkerBee.X5 + (-1.0 + _rnd.NextDouble() * 2.0) * (curWorkerBee.X5 - neighborFoodSource.X5),
                                _intervals.X5Low, _intervals.X5Up);
                        break;
                    case 6:
                        neighborFoodSource.X6 =
                            Math.Clamp(
                                curWorkerBee.X6 + (-1.0 + _rnd.NextDouble() * 2.0) * (curWorkerBee.X6 - neighborFoodSource.X6),
                                _intervals.X6Low, _intervals.X6Up);
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