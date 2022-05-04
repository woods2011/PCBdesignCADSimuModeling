using System.Windows.Input;
using PcbDesignSimuModeling.Core.Commands;

namespace PcbDesignSimuModeling.Core.ViewModels;

public class MessageViewModel : BaseViewModel
{
    public string Message { get; set; } = String.Empty;

    public bool HasMessage => !String.IsNullOrEmpty(Message);

    public ICommand OkCommand => new ActionCommand(_ => Message = String.Empty);
        

    public override string Error => String.Empty;
    public override bool IsValid => true;
    public override string this[string columnName] => String.Empty;
}