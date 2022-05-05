using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PcbDesignSimuModeling.Core.Models.Resources.Ram;

public class Ram : MixedResource, INotifyPropertyChanged
{
    private double _totalAmount;
    private readonly Dictionary<int, double> _requestIdAndAllocatedAmount = new();
    protected override List<int> UtilizingRequestsIds => _requestIdAndAllocatedAmount.Keys.ToList();

    public double TotalAmount
    {
        get => _totalAmount;
        set
        {
            _totalAmount = value;
            AvailableAmount = _totalAmount;
        }
    }
    public double AvailableAmount { get; private set; }

    
    public Ram(double totalAmount) => TotalAmount = totalAmount;


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


    public override decimal Cost => Convert.ToDecimal(TotalAmount) * 450;
    public override IResource Clone() => new Ram(TotalAmount);
    public event PropertyChangedEventHandler? PropertyChanged;
}