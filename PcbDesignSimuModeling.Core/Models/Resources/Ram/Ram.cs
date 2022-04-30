using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PcbDesignSimuModeling.Core.Models.Resources.Ram;

public class Ram : SharedResource, INotifyPropertyChanged
{
    private double _availableAmount;
    private int _totalAmount;
    private readonly Dictionary<int, double> _requestIdAndAllocatedAmount = new();

    public int TotalAmount
    {
        get => _totalAmount;
        set
        {
            _totalAmount = value;
            _availableAmount = _totalAmount;
        }
    }

    public bool TryGetResource(int requestId, double amount)
    {
        if (_availableAmount - amount < 1e-12) return false;
        
        _requestIdAndAllocatedAmount[requestId] = amount;
        _availableAmount -= amount;
        return true;
    }

    public override double ResValueForProc(int requestId) => _requestIdAndAllocatedAmount[requestId];

    public override void FreeResource(int requestId)
    {
        _requestIdAndAllocatedAmount.Remove(requestId, out var amount);
        _availableAmount += amount;
    }

    public override IResource Clone() => new Ram {TotalAmount = TotalAmount};

    public override decimal Cost { get; }

    public event PropertyChangedEventHandler? PropertyChanged;
}