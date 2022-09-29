using System;
using System.Windows.Input;
using PcbDesignSimuModeling.WPF.Commands;
using PcbDesignSimuModeling.WPF.ViewModels.Helpers;
using PcbDesignSimuModeling.WPF.ViewModels.OptimizationModule;
using PcbDesignSimuModeling.WPF.ViewModels.SimuSystem;

namespace PcbDesignSimuModeling.WPF.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly SimuSystemViewModel _simuSystemVm;
    private readonly OptimizationModuleViewModel _optimizationModuleVm;
    private BaseViewModel _selectedViewModel;

    public MessageViewModel MessageViewModel { get; } = new();

    public MainViewModel()
    {
        _simuSystemVm = new SimuSystemViewModel(messageViewModel: MessageViewModel);
        _optimizationModuleVm = new OptimizationModuleViewModel(messageViewModel: MessageViewModel);
        _selectedViewModel = _simuSystemVm;

        _optimizationModuleVm.ShareConfiguration += _simuSystemVm.ImportConfigFromOptimizationModule;
    }

    public BaseViewModel SelectedViewModel
    {
        get => _selectedViewModel;
        set
        {
            _selectedViewModel = value;
            (_selectedViewModel as SimuSystemViewModel)?.CadConfigExpenseReportVm.RecalculateConfig();
        }
    }

    public ICommand UpdateViewCommand => new ActionCommand(param => UpdateView(param?.ToString()));

    private void UpdateView(string? param)
    {
        if (param is null) return;

        SelectedViewModel = param switch
        {
            "SimuSystem" => _simuSystemVm,
            "OptimizationModule" => _optimizationModuleVm,
            "Settings" => new GeneralSimulationSettingsVm(),
            _ => SelectedViewModel
        };
    }

    public override string Error => String.Empty;
    public override bool IsValid => true;
    public override string this[string columnName] => String.Empty;
}