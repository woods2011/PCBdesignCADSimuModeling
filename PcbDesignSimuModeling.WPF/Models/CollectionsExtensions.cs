using System;
using System.Collections.Generic;
using System.Linq;

namespace PcbDesignSimuModeling.WPF.Models;

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

    public static IEnumerable<TResult> SelectWithPrevious<TSource, TResult>
        (this IEnumerable<TSource> source, Func<TSource, TSource, TResult> projection)
    {
        using var iterator = source.GetEnumerator();
        if (!iterator.MoveNext()) yield break;

        var previous = iterator.Current;
        while (iterator.MoveNext())
        {
            yield return projection(previous, iterator.Current);
            previous = iterator.Current;
        }
    }

    public static IEnumerable<TResult> SelectWithPreviousCustomFirst<TSource, TResult>
        (this IEnumerable<TSource> source, TSource first, Func<TSource, TSource, TResult> projection)
    {
        using var iterator = source.GetEnumerator();

        var previous = first;
        while (iterator.MoveNext())
        {
            yield return projection(previous, iterator.Current);
            previous = iterator.Current;
        }
    }
}

public static class MathExt
{
    public static TimeSpan RoundSec(this TimeSpan time) => TimeSpan.FromSeconds(Math.Round(time.TotalSeconds));
    public static TimeSpan RoundMin(this TimeSpan time) => TimeSpan.FromMinutes(Math.Round(time.TotalMinutes));
    public static TimeSpan RoundHours(this TimeSpan time) => TimeSpan.FromHours(Math.Round(time.TotalHours));

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

    public static decimal WithMonthAmort(this decimal price, decimal amortizationPeriodYears) =>
        price / (amortizationPeriodYears * 12);

    public static TimeSpan ToWorkDays(this TimeSpan netWorkTime)
    {
        var (div, rem) = netWorkTime.TotalHours.DivRem(8);
        return TimeSpan.FromHours(24 * div + rem + 10);
    }

    public static TimeSpan ToWorkWeek(this TimeSpan workTime)
    {
        var (div, rem) = workTime.TotalDays.DivRem(5);
        return TimeSpan.FromDays(7 * div + rem);
    }

    public static TimeSpan ToWorkWeekFull(this TimeSpan netWorkTime) => netWorkTime.ToWorkDays().ToWorkWeek();

    public static TimeSpan FromWorkDays(this TimeSpan netWorkTime)
    {
        var (div, rem) = (netWorkTime.TotalHours - 10).DivRem(24);
        return TimeSpan.FromHours(8 * div + Math.Min(rem, 8.0 - 1e-5));
    }

    public static TimeSpan FromWorkWeek(this TimeSpan workTime)
    {
        var (div, rem) = workTime.TotalDays.DivRem(7);
        return TimeSpan.FromDays(5 * div + Math.Min(rem, 5.0 - 1e-5));
    }

    public static TimeSpan FromWorkWeekFull(this TimeSpan workTime) => workTime.FromWorkWeek().FromWorkDays();
}