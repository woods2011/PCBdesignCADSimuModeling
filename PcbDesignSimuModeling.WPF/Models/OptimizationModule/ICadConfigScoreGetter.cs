using System;

namespace PcbDesignSimuModeling.WPF.Models.OptimizationModule;

public interface ICadConfigScoreGetter
{
    double GetScore(double totalConfigCost, double avgProductionTimeHours);
}

public class CadConfigScoreGetter1 : ICadConfigScoreGetter
{
    public double PreferredTimeDays { get; init; } = 5.0;

    public double GetScore(double totalConfigCost, double avgProductionTimeHours) =>
        (0.6 + 0.4 * 100000 / totalConfigCost) *
        (0.4 + 0.6 / Math.Exp(Math.Max(0, avgProductionTimeHours - PreferredTimeDays)));
}

public class CadConfigScoreGetter2 : ICadConfigScoreGetter
{
    public CadConfigScoreGetter2(double goalProductionTimeHours) => GoalProductionTimeHours = goalProductionTimeHours;

    public double GoalProductionTimeHours { get; }

    public double GetScore(double totalConfigCost, double avgProductionTimeHours) =>
        1e6 / totalConfigCost / Math.Exp(Math.Max(0, (avgProductionTimeHours - GoalProductionTimeHours) / 24.0));
}