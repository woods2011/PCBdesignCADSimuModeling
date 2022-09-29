using System;
using System.Collections.Generic;
using PcbDesignSimuModeling.WPF.Models.Resources.EadSybSystems.PlacingSystems;
using PcbDesignSimuModeling.WPF.Models.Resources.EadSybSystems.RoutingSystems;
using PcbDesignSimuModeling.WPF.ViewModels.Helpers;
using PcbDesignSimuModeling.WPF.ViewModels.Resources;
using static PcbDesignSimuModeling.WPF.Models.SimuSystem.GeneralSimulationSettings;

namespace PcbDesignSimuModeling.WPF.ViewModels.SimuSystem;

public class CadConfigurationVm : BaseViewModel
{
    public IReadOnlyList<string> PlacingAlgStrList { get; } = new List<string>
        {PlacingSysFactoryProvider.PlacingToporStr};

    public string SelectedPlacingAlgStr { get; set; } = PlacingSysFactoryProvider.PlacingToporStr;

    public IReadOnlyList<string> WireRoutingAlgStrList { get; } =
        new List<string> {RoutingSysFactoryProvider.RoutingToporStr};

    public string SelectedWireRoutingAlgStr { get; set; } = RoutingSysFactoryProvider.RoutingToporStr;

    public CpuClusterVm Cpu { get; private set; } = new(16, 2.5);
    public RamVm Ram { get; private set; } = new(16);
    public ServerVm Server { get; private set; } = new(150);
    public int DesignersCount { get; set; } = 2;
    public decimal DesignersSumSalary => DesignersCount * CurSettings.DesignerSalary;


    public CadConfigurationEm Export() => new(
        cpu: Cpu,
        ram: Ram,
        server: Server,
        designersCount: DesignersCount,
        selectedPlacingAlgStr: SelectedPlacingAlgStr,
        selectedWireRoutingAlgStr: SelectedWireRoutingAlgStr
    );

    public void Import(CadConfigurationEm cadConfigurationEm)
    {
        Cpu = cadConfigurationEm.Cpu;
        Ram = cadConfigurationEm.Ram;
        Server = cadConfigurationEm.Server;
        DesignersCount = cadConfigurationEm.DesignersCount;
        SelectedPlacingAlgStr = cadConfigurationEm.SelectedPlacingAlgStr;
        SelectedWireRoutingAlgStr = cadConfigurationEm.SelectedWireRoutingAlgStr;
    }

    public override string Error => String.Empty;
    public override bool IsValid => true;
    public override string this[string columnName] => String.Empty;
}