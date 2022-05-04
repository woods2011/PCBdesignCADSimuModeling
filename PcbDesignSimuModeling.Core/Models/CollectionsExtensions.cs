using PcbDesignSimuModeling.Core.Models.Resources;

namespace PcbDesignSimuModeling.Core.Models;

public static class CollectionsExtensions
{
    public static (TSource, TKey) MinByAndKey<TSource, TKey>(this IEnumerable<TSource> source,
        Func<TSource, TKey> selector, IComparer<TKey>? comparer = null)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        comparer ??= Comparer<TKey>.Default;

        using var sourceIterator = source.GetEnumerator();
        if (!sourceIterator.MoveNext()) throw new InvalidOperationException("Sequence contains no elements");

        var min = sourceIterator.Current;
        var minKey = selector(min);
        while (sourceIterator.MoveNext())
        {
            var candidate = sourceIterator.Current;
            var candidateProjected = selector(candidate);

            if (comparer.Compare(candidateProjected, minKey) >= 0) continue;

            min = candidate;
            minKey = candidateProjected;
        }

        return (min, minKey);
    }

    public static List<TSource> MinsBy<TSource, TKey>(this IEnumerable<TSource> source,
        Func<TSource, TKey> selector, IComparer<TKey>? comparer = null)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        comparer ??= Comparer<TKey>.Default;

        using var sourceIterator = source.GetEnumerator();
        if (!sourceIterator.MoveNext()) throw new InvalidOperationException("Sequence contains no elements");

        var first = sourceIterator.Current;
        var minKey = selector(first);
        var minimums = new List<TSource>() {first};
        while (sourceIterator.MoveNext())
        {
            var candidate = sourceIterator.Current;
            var candidateProjected = selector(candidate);

            switch (comparer.Compare(candidateProjected, minKey))
            {
                case > 0: continue;
                case 0:
                    minimums.Add(candidate);
                    continue;
            }

            minKey = candidateProjected;
            minimums.Clear();
            minimums.Add(candidate);
        }

        return minimums;
    }

    public static (List<TSource>, TKey) MinsByAndKey<TSource, TKey>(this IEnumerable<TSource> source,
        Func<TSource, TKey> selector, IComparer<TKey>? comparer = null)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        comparer ??= Comparer<TKey>.Default;

        using var sourceIterator = source.GetEnumerator();
        if (!sourceIterator.MoveNext()) throw new InvalidOperationException("Sequence contains no elements");

        var first = sourceIterator.Current;
        var minKey = selector(first);
        var minimums = new List<TSource>() {first};
        while (sourceIterator.MoveNext())
        {
            var candidate = sourceIterator.Current;
            var candidateProjected = selector(candidate);

            switch (comparer.Compare(candidateProjected, minKey))
            {
                case > 0: continue;
                case 0:
                    minimums.Add(candidate);
                    continue;
            }

            minKey = candidateProjected;
            minimums.Clear();
            minimums.Add(candidate);
        }

        return (minimums, minKey);
    }

    public static void InsertAfterCondition<TSource>(this IList<TSource> source, TSource item,
        Func<TSource, bool> predicate)
    {
        var insertIndex = source.TakeWhile(predicate).Count();
        source.Insert(insertIndex, item);
    }

    public static void InsertRangeAfterCondition<TSource>(this IList<TSource> source, IEnumerable<TSource> items,
        Func<TSource, TSource, bool> predicate) 
    {
        foreach (var insItem in items) 
            InsertAfterCondition(source, insItem, itemSource => predicate(itemSource, insItem));
    }

    public static IEnumerable<double> CumulativeSum(this IEnumerable<double> numbers)
    {
        var acc = 0.0;

        foreach (var number in numbers)
        {
            acc += number;
            yield return acc;
        }
    }
}

public static class MathExt
{
    public static TimeSpan MultiplyAndClamp(this TimeSpan time, double mulFactor, TimeSpan maxTime)
    {
        if (maxTime.Ticks / mulFactor > time.Ticks)
            return time * mulFactor;
        return maxTime;
    }

    public static TimeSpan MultiplyAndClamp(this TimeSpan time, double mulFactor) =>
        time.MultiplyAndClamp(mulFactor, TimeSpan.MaxValue);


    public static TimeSpan AddAndClamp(this TimeSpan time, TimeSpan addTime, TimeSpan maxTime)
    {
        if (maxTime - addTime > time)
            return time + addTime;
        return maxTime;
    }

    public static TimeSpan AddAndClamp(this TimeSpan time, TimeSpan addTime) =>
        time.AddAndClamp(addTime, TimeSpan.MaxValue);

    public static (int Quotient, double Remainder) DivRem(this double left, double right)
    {
        var quotient = (int) (left / right);
        return (quotient, left - quotient * right);
    }
}