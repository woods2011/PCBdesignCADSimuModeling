using System;
using System.Windows.Input;

namespace PCBdesignCADSimuModeling.Commands
{
    public sealed class ActionCommand : ICommand
    {
        private readonly Action<object> _action;
        private readonly Predicate<object> _predicate;


        /// <summary>
        /// Initializes a new instance of the <see cref="ActionCommand"/> class.
        /// </summary>
        /// <param name="action">The <see cref="Action"/> delegate to wrap.</param>
        /// <param name="predicate">The <see cref="Predicate{Object}"/> that determines whether the action delegate may be invoked.</param>
        public ActionCommand(Action<object> action, Predicate<object> predicate = null)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action), "You must specify an Action<T>.");
            _predicate = predicate;
        }

        
        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <returns>
        /// true if this command can be executed; otherwise, false.
        /// </returns>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        public bool CanExecute(object parameter) => _predicate == null || _predicate(parameter);

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        public void Execute(object parameter = null) => _action(parameter);

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
