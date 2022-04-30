namespace PcbDesignSimuModeling.Core.Models.Resources.Algorithms;

public abstract class PcbAlgorithm : IPcbAlgorithm
{
    private readonly long _totalComplexity;
    private double _completionRate = 0.0;

       
    protected PcbAlgorithm(long totalComplexity, int maxThreadUtilization)
    {
        _totalComplexity = totalComplexity;
        MaxThreadUtilization = maxThreadUtilization;
    }
        

    public int MaxThreadUtilization { get; }

    private double CompletionRate
    {
        get => _completionRate;
        set => _completionRate = Math.Clamp(value, 0.0, 1.0);
    }

    public bool IsComplete => Math.Abs(CompletionRate - 1.0) < 0.00001;


    public virtual void UpdateModelTime(TimeSpan deltaTime, double cpuPower)
    {
        CompletionRate += deltaTime.TotalSeconds * cpuPower / _totalComplexity;
    }

    public virtual TimeSpan EstimateEndTime(double cpuPower)
    {
        return TimeSpan.FromSeconds(Math.Min(TimeSpan.MaxValue.TotalSeconds / 2.0, Math.Round((1 - CompletionRate) * _totalComplexity / cpuPower)));
    }
}