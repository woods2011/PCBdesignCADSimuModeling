using Newtonsoft.Json;
using PcbDesignSimuModeling.WPF.Models.OptimizationModule;
using PcbDesignSimuModeling.WPF.Models.Resources.CpuClusterRes;
using PcbDesignSimuModeling.WPF.Models.Resources.RamRes;
using PcbDesignSimuModeling.WPF.Models.Resources.ServerRes;
using PcbDesignSimuModeling.WPF.ViewModels.Resources;

namespace PcbDesignSimuModeling.WPF.ViewModels.SimuSystem;

public class CadConfigurationEm
{
    [JsonProperty("Процессорная Конфигурация Вычислитительного Сервера")]
    public CpuClusterVm Cpu { get; set; }
    
    [JsonProperty("Оперативная Память")]
    public RamVm Ram { get; set; }
    
    [JsonProperty("Интернет Соединение")]
    public ServerVm Server { get; set; }
    
    [JsonProperty("Число Проектировщиков")]
    public int DesignersCount { get; set; }
    
    [JsonProperty("Выбранная Подсистема Автоматического Размещения")]
    public string SelectedPlacingAlgStr { get; set; }
    
    [JsonProperty("Выбранная Подсистема Автоматической Трассировки")]
    public string SelectedWireRoutingAlgStr { get; set; }

    [JsonConstructor]
    public CadConfigurationEm(CpuClusterVm cpu, RamVm ram, ServerVm server, int designersCount,
        string selectedPlacingAlgStr, string selectedWireRoutingAlgStr)
    {
        Cpu = cpu;
        Ram = ram;
        Server = server;
        DesignersCount = designersCount;
        SelectedPlacingAlgStr = selectedPlacingAlgStr;
        SelectedWireRoutingAlgStr = selectedWireRoutingAlgStr;
    }
    
    public CadConfigurationEm(CpuCluster cpu, Ram ram, Server server, int designersCount,
        string selectedPlacingAlgStr, string selectedWireRoutingAlgStr)
    {
        Cpu = CpuClusterVm.FromModel(cpu);
        Ram = RamVm.FromModel(ram);
        Server = ServerVm.FromModel(server);
        DesignersCount = designersCount;
        SelectedPlacingAlgStr = selectedPlacingAlgStr;
        SelectedWireRoutingAlgStr = selectedWireRoutingAlgStr;
    }
    
    public CadConfigurationEm(FoodSource foodSource)
    {
        Cpu = new CpuClusterVm(foodSource.ThreadsCount, foodSource.ClockRate);
        Ram = new RamVm(foodSource.RamAmount);
        Server = new ServerVm(foodSource.ServerSpeed);
        DesignersCount = foodSource.DesignersCount;
        SelectedPlacingAlgStr = foodSource.PlacingAlgStr;
        SelectedWireRoutingAlgStr = foodSource.WireRoutingAlgStr;
    }
}