using System.Collections;

namespace PcbDesignSimuModeling.Core.Models.Resources.Cpu.SubClasses;

public class ThreadUtilizationList : IList<ThreadUtilizationByProcess>
{
    private readonly List<ThreadUtilizationByProcess> _listImplementation;

    public ThreadUtilizationList() => _listImplementation = new List<ThreadUtilizationByProcess>();

    public ThreadUtilizationList(IEnumerable<ThreadUtilizationByProcess> collection) =>
        _listImplementation = new List<ThreadUtilizationByProcess>(collection);

    public ThreadUtilizationList(int capacity) => _listImplementation = new List<ThreadUtilizationByProcess>(capacity);


    public void Add(ThreadUtilizationByProcess item)
    {
        var index = _listImplementation
            .TakeWhile(threadByProcUtilization => item.MaxUtil - 1e-12 > threadByProcUtilization.MaxUtil).Count();
        _listImplementation.Insert(index, item);

        RecalculateUtilization();
    }

    public void AddRange(IEnumerable<ThreadUtilizationByProcess> collection)
    {
        _listImplementation.AddRange(collection);
        RecalculateUtilization();
    }

    public bool Remove(ThreadUtilizationByProcess item)
    {
        var result = _listImplementation.Remove(item);
        if (result) RecalculateUtilization();
        return result;
    }

    public int RemoveRange(IEnumerable<ThreadUtilizationByProcess> items)
    {
        var deletedCount = items.Count(item => _listImplementation.Remove(item));
        if (deletedCount > 0) RecalculateUtilization();
        return deletedCount;
    }

    public int RemoveAll(Predicate<ThreadUtilizationByProcess> match)
    {
        var result = _listImplementation.RemoveAll(match);
        if (result > 0) RecalculateUtilization();
        return result;
    }

    public void RecalculateUtilization()
    {
        var taskCount = ActiveTasks;
        if (taskCount < 1)
        {
            RealTaskCount = 0.0;
            IdlePercent = 1.0;
            return;
        }

        _listImplementation.ForEach(utilization => utilization.CurUtil = 1.0 / taskCount);

        var shift = 0;
        while (shift < taskCount)
        {
            var overheadSum = 0.0;

            while (shift < taskCount)
            {
                var utilization = _listImplementation[shift];
                var utilOverhead = utilization.CurUtil - utilization.MaxUtil;
                if (utilOverhead <= 1e-12) break;

                utilization.CurUtil -= utilOverhead;
                overheadSum += utilOverhead;
                shift++;
            }

            if (overheadSum <= 1e-12) break;
            var overheadAvg = overheadSum / (taskCount - shift);

            for (var i = shift; i < taskCount; i++)
                _listImplementation[i].CurUtil += overheadAvg;
        }

        RealTaskCount = 1.0 + _listImplementation.Sum(utilization => utilization.MaxUtil - 1.0 / taskCount);
        IdlePercent = Math.Max(0.0, 1.0 - RealTaskCount);
    }

    public double IdlePercent { get; private set; }

    public double RealTaskCount { get; private set; }

    public int ActiveTasks => Count;

    #region DelegateToList

    public IEnumerator<ThreadUtilizationByProcess> GetEnumerator() => _listImplementation.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _listImplementation).GetEnumerator();

    public void Clear() => _listImplementation.Clear();

    public bool Contains(ThreadUtilizationByProcess item) => _listImplementation.Contains(item);

    public void CopyTo(ThreadUtilizationByProcess[] array, int arrayIndex) =>
        _listImplementation.CopyTo(array, arrayIndex);

    public int Count => _listImplementation.Count;

    public bool IsReadOnly => ((IList<ThreadUtilizationByProcess>) _listImplementation).IsReadOnly;

    public int IndexOf(ThreadUtilizationByProcess item) => _listImplementation.IndexOf(item);

    public void Insert(int index, ThreadUtilizationByProcess item) => _listImplementation.Insert(index, item);

    public void RemoveAt(int index) => _listImplementation.RemoveAt(index);

    public ThreadUtilizationByProcess this[int index]
    {
        get => _listImplementation[index];
        set => _listImplementation[index] = value;
    }

    #endregion
}

public static class ThreadUtilizationListExtensions
{
    internal static ThreadUtilizationList ToThreadUtilizationList(
        this IEnumerable<ThreadUtilizationByProcess> enumerable) => new(enumerable);
}