using System;
using System.Windows.Input;
using PCBdesignCADSimuModeling.Commands;

namespace PCBdesignCADSimuModeling.ViewModels
{
    public class MessageViewModel : BaseViewModel
    {
        private string _message;


        public MessageViewModel()
        {
        }

        
        public string Message
        {
            get => _message;
            set
            {
                OnPropertyChanged(ref _message, value);
                OnPropertyChanged(nameof(HasMessage));
            }
        }

        public bool HasMessage => !String.IsNullOrEmpty(Message);


        public ICommand OkCommand => new ActionCommand(_ => Message = String.Empty);


        public override string Error => String.Empty;
        public override bool IsValid => true;
        public override string this[string columnName] => String.Empty;
    }
}