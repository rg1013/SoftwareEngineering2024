/******************************************************************************
 * Filename    = RelayCommand.cs
 *
 * Author      = Sai Hemanth Reddy
 *
 * Product     = PlexShare
 * 
 * Project     = FileCloner
 *
 * Description = Implements ICommand interface to allow binding actions to UI commands.
 *****************************************************************************/
using System;
using System.Windows.Input;

namespace FileCloner.ViewModels
{
    /// <summary>
    /// RelayCommand class to encapsulate command logic for MVVM binding.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        /// <summary>
        /// Initializes a new instance of the RelayCommand class.
        /// </summary>
        /// <param name="execute">Action to execute on command.</param>
        /// <param name="canExecute">Function to determine if command can execute.</param>
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Command parameter.</param>
        /// <returns>True if command can execute, otherwise false.</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">Command parameter.</param>
        public void Execute(object parameter)
        {
            _execute();
        }

        /// <summary>
        /// Raises the CanExecuteChanged event to notify that command execution status has changed.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
