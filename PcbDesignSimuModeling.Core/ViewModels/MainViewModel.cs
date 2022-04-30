using System.Windows.Input;
using PcbDesignSimuModeling.Core.Commands;

namespace PcbDesignSimuModeling.Core.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly SimuSystemViewModel _simuSystemVm;
    private readonly OptimizationModuleViewModel _optimizationModuleVm;

    public MessageViewModel MessageViewModel { get; } = new();

    public MainViewModel()
    {
        _simuSystemVm = new SimuSystemViewModel { MessageViewModel = MessageViewModel };
        _optimizationModuleVm = new OptimizationModuleViewModel { MessageViewModel = MessageViewModel };
        SelectedViewModel = _simuSystemVm;
    }

    public BaseViewModel SelectedViewModel { get; set; }

    public ICommand UpdateViewCommand => new ActionCommand(param => UpdateView(param?.ToString()));

    private void UpdateView(string? param)
    {
        if (param is null) return;

        SelectedViewModel = param switch
        {
            "SimuSystem" => _simuSystemVm,
            "OptimizationModule" => _optimizationModuleVm,
            _ => SelectedViewModel
        };
    }
}