using System;
using System.Windows.Input;

namespace PcbDesignCADSimuModeling.Commands
{
    public class ActionCommand : ICommand
    {
        private readonly Action<object?> _action;
        private readonly Predicate<object?>? _predicate;


        public ActionCommand(Action<object?> action, Predicate<object?>? predicate = null)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action), "You must specify an Action<T>.");
            _predicate = predicate;
        }


        public bool CanExecute(object? parameter) => _predicate == null || _predicate(parameter);

    
        public void Execute(object? parameter = null) => _action(parameter);

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}