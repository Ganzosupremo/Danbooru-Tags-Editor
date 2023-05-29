using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace DanTagsEditor.Core
{
    public class RelayCommand : ICommand
    {
        private Action<object> _execute;
        private Func<object, bool>? _canExecute;

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> execute, Func<object, bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter!);
        }

        public void Execute(object? parameter)
        {
            //if (IsNotNull(parameter))
              _execute(parameter!);
        }

        //private static bool IsNotNull([NotNullWhen(true)] object? obj) => obj != null;
    }
}
