using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UXModule.ViewModel
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> mAction;

        public RelayCommand(Action<object> action)
        {
            mAction = action;
        }

        public event EventHandler CanExecuteChanged = (sender, e) => { };

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) => mAction(parameter);
    }
}
