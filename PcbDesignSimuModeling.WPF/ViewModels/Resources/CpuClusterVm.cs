using System.ComponentModel;
using Newtonsoft.Json;
using PcbDesignSimuModeling.WPF.Models.Resources.CpuClusterRes;

namespace PcbDesignSimuModeling.WPF.ViewModels.Resources;

public class CpuClusterVm : INotifyPropertyChanged
{
    [JsonProperty("Число Потоков")]
    public int ThreadCount { get; set; }
    
    [JsonProperty("Макс. Частота")]
    public double ClockRate { get; set; }

    public CpuClusterVm(int threadCount, double clockRate)
    {
        ThreadCount = threadCount;
        ClockRate = clockRate;
    }

    [JsonIgnore] public CpuCluster Model => new(ThreadCount, ClockRate);

    public CpuCluster ToModel() => Model;
    public static CpuClusterVm FromModel(CpuCluster model) => new(model.ThreadCount, model.ClockRate);

    public event PropertyChangedEventHandler? PropertyChanged;
}