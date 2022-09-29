using System.ComponentModel;
using Newtonsoft.Json;
using PcbDesignSimuModeling.WPF.Models.Resources.ServerRes;

namespace PcbDesignSimuModeling.WPF.ViewModels.Resources;

public class ServerVm : INotifyPropertyChanged
{
    [JsonProperty("Скорость (Мбит/c)")]
    public double InternetSpeed { get; set; }

    public ServerVm(double internetSpeed) => InternetSpeed = internetSpeed;

    [JsonIgnore] public Server Model => new(InternetSpeed);
    public Server ToModel() => Model;
    public static ServerVm FromModel(Server model) => new(model.InternetSpeed); 
    
    public event PropertyChangedEventHandler? PropertyChanged;
}