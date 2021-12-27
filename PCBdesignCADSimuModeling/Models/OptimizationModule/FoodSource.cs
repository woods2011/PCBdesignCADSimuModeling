using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using PcbDesignCADSimuModeling.Models.Resources.Algorithms.PlacingAlgorithms;
using PcbDesignCADSimuModeling.Models.Resources.Algorithms.WireRoutingAlgorithms;

namespace PcbDesignCADSimuModeling.Models.OptimizationModule
{
    public class FoodSource
    {
        public int ThreadsCount { get; set; }
        public double ClockRate { get; set; }

        public double ServerSpeed { get; set; }

        [JsonIgnore]
        public int PlacingAlgIndex { get; set; }

        [JsonIgnore]
        public int WireRoutingAlgIndex { get; set; }

        public int DesignersCount { get; set; }

        public FoodSource(int threadsCount, double clockRate, double serverSpeed, int placingAlgIndex,
            int wireRoutingAlgIndex, int designersCount)
        {
            ThreadsCount = threadsCount;
            ClockRate = clockRate;
            ServerSpeed = serverSpeed;
            PlacingAlgIndex = placingAlgIndex;
            WireRoutingAlgIndex = wireRoutingAlgIndex;
            DesignersCount = designersCount;
        }

        public double FuncValue { get; set; }

        public int NumberOfVisits { get; set; } = 0;

        public FoodSource(int threadsCount, double clockRate, double serverSpeed,
            int placingAlgIndex, int wireRoutingAlgIndex, int designersCount,
            Func<int, double, double, int, int, int, double> objectiveFunction)
            : this(threadsCount, clockRate, serverSpeed, placingAlgIndex, wireRoutingAlgIndex, designersCount) =>
            FuncValue = objectiveFunction(threadsCount, clockRate, serverSpeed, placingAlgIndex, wireRoutingAlgIndex,
                designersCount);

        public void CalculateCost(Func<int, double, double, int, int, int, double> objectiveFunction) =>
            FuncValue = objectiveFunction(ThreadsCount, ClockRate, ServerSpeed, PlacingAlgIndex, WireRoutingAlgIndex,
                DesignersCount);

        public FoodSource Copy() => (FoodSource)MemberwiseClone();

        public override string ToString()
        {
            return $"Оценка: {FuncValue}{Environment.NewLine}" +
                   $"Конфигурация:{Environment.NewLine}" +
                   $"   Потоков: {ThreadsCount} ; Частота: {ClockRate}{Environment.NewLine}" +
                   $"   Скорость сервера: {ServerSpeed}{Environment.NewLine}" +
                   $"   Размещение: {PlacingAlgProviderFactory.AlgIndexNameMap[PlacingAlgIndex]} ; Трассировка: {WireRoutingAlgProviderFactory.AlgIndexNameMap[WireRoutingAlgIndex]}{Environment.NewLine}" +
                   $"   Количество проектировщиков: {DesignersCount}{Environment.NewLine}";
        }


        public string PlacingAlgStr => PlacingAlgProviderFactory.AlgIndexNameMap[PlacingAlgIndex];
        public string WireRoutingAlgStr => WireRoutingAlgProviderFactory.AlgIndexNameMap[WireRoutingAlgIndex];
    }


    public static class FoodSourceExtensions
    {
        public static IEnumerable<T> FisherYatesShuffle<T>(this IEnumerable<T> enumerableToShuffle, Random rnd)
        {
            var shuffledArray = enumerableToShuffle.ToArray();

            for (var i1 = 0; i1 < shuffledArray.Length; i1++)
            {
                var i2 = rnd.Next(i1, shuffledArray.Length);
                yield return shuffledArray[i2];
                shuffledArray[i2] = shuffledArray[i1];
            }
        }

        public static IEnumerable<T> FisherYatesShuffleExcept<T>(this IEnumerable<T> enumerableToShuffle, T except,
            Random rnd)
        {
            var shuffledArray = enumerableToShuffle.ToArray();
            if (shuffledArray.Length < 2) yield break;

            var indexExcepted = Array.IndexOf(shuffledArray, except);
            shuffledArray[indexExcepted] = shuffledArray[0];

            for (var i1 = 1; i1 < shuffledArray.Length; i1++)
            {
                var i2 = rnd.Next(i1, shuffledArray.Length);
                yield return shuffledArray[i2];
                shuffledArray[i2] = shuffledArray[i1];
            }
        }

        public static IEnumerable<(T, T)> GetParentPairs<T>(this IList<T> iList, Random rnd)
        {
            var indexPairs = new (int, int)[(iList.Count * (iList.Count - 1)) / 2];

            var z = 0;
            for (var i = 0; i < iList.Count - 1; i++)
            {
                for (var j = i + 1; j < iList.Count; j++)
                {
                    indexPairs[z] = (i, j);
                    z++;
                }
            }

            return indexPairs.FisherYatesShuffle(rnd)
                .Select(indexPair => (iList[indexPair.Item1], iList[indexPair.Item2]));
        }
    }
}