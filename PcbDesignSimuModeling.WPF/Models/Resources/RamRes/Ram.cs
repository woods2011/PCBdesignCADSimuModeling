using System;
using System.Collections.Generic;
using System.Linq;
using static PcbDesignSimuModeling.WPF.Models.SimuSystem.GeneralSimulationSettings;

namespace PcbDesignSimuModeling.WPF.Models.Resources.RamRes;

public class Ram : MixedResource
{
    private readonly Dictionary<int, double> _requestIdAndAllocatedAmount = new();
    protected override List<int> UtilizingRequestsIds => _requestIdAndAllocatedAmount.Keys.ToList();

    public double TotalAmount { get; }
    public double AvailableAmount { get; private set; }


    public Ram(double totalAmount)
    {
        TotalAmount = totalAmount;
        AvailableAmount = totalAmount;

        Cost = Convert.ToDecimal(TotalAmount) * CurSettings.RamPerGigAvgPrice;
        CostPerMonth = Cost.WithMonthAmort(CurSettings.RamAmortization);
    }

    public decimal Cost { get; }
    public override decimal CostPerMonth { get; }

    public bool TryGetResource(int requestId, double amount)
    {
        if (AvailableAmount - amount < 1e-12) return false;

        _requestIdAndAllocatedAmount[requestId] = amount;
        AvailableAmount -= amount;
        return true;
    }

    public override double PowerForRequest(int requestId) => _requestIdAndAllocatedAmount[requestId];

    public override void FreeResource(int requestId)
    {
        _requestIdAndAllocatedAmount.Remove(requestId, out var amount);
        AvailableAmount += amount;
    }
}