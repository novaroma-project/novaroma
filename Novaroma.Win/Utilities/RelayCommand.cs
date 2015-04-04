using System;
using System.Windows.Input;

namespace Novaroma.Win.Utilities {

    public class RelayCommand : ICommand {
        readonly Action<object> _execute;
        readonly Predicate<object> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null) {
            _execute = param => execute();
            if (canExecute != null)
                _canExecute = param => canExecute();
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null) {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) {
            return _canExecute == null || _canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter) {
            _execute(parameter);
        }
    }
}