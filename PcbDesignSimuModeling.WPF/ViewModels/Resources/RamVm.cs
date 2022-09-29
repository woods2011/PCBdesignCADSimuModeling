using System.ComponentModel;
using Newtonsoft.Json;
using PcbDesignSimuModeling.WPF.Models.Resources.RamRes;

namespace PcbDesignSimuModeling.WPF.ViewModels.Resources;

public class RamVm : INotifyPropertyChanged
{
    [JsonProperty("Объем (Гб)")]
    public double TotalAmount { get; set; }

    public RamVm(double totalAmount) => TotalAmount = totalAmount;

    [JsonIgnore] public Ram Model => new(TotalAmount);
    
    public Ram ToModel() => Model;
    public static RamVm FromModel(Ram model) => new(model.TotalAmount); 
    
    public event PropertyChangedEventHandler? PropertyChanged;
}