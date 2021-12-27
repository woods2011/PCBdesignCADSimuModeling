using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace PcbDesignCADSimuModeling.Models.OptimizationModule
{
    public class FoodSource : IComparable<FoodSource>
    {
        public double X1 { get; set; }
        public double X2 { get; set; }

        public double X3 { get; set; }

        public double X4 { get; set; }

        public double X5 { get; set; }

        public double X6 { get; set; }

        public FoodSource(double x1, double x2, double x3, double x4, double x5, double x6)
        {
            X1 = x1;
            X2 = x2;
            X3 = x3;
            X4 = x4;
            X5 = x5;
            X6 = x6;
        }

        [JsonProperty("Function Value")]
        public double Cost { get; set; }

        public int CurNumOfUses { get; set; }

        public FoodSource(double x1, double x2, double x3, double x4, double x5, double x6,
            Func<double, double, double, double, double, double, double> objectiveFunction)
            : this(x1, x2, x3, x4, x5, x6) => Cost = objectiveFunction(x1, x2, x3, x4, x5, x6);

        public void CalculateCost(Func<double, double, double, double, double, double, double> objectiveFunction) =>
            Cost = objectiveFunction(X1, X2, X3, X4, X5, X6);


        public int CompareTo(FoodSource? otherBacteria)
        {
            if (otherBacteria == null) throw new ArgumentNullException(nameof(otherBacteria));

            if (Cost > otherBacteria.Cost) return 1;
            if (Cost < otherBacteria.Cost) return -1;
            return 0;
        }

        public FoodSource Copy() => (FoodSource)MemberwiseClone();
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