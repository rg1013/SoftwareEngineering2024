/******************************************************************************
 * Filename    = ViewModelBase.cs
 *
 * Author      = Sai Hemanth Reddy
 *
 * Product     = PlexShare
 * 
 * Project     = FileCloner
 *
 * Description = Base class for ViewModels implementing INotifyPropertyChanged for
 *               property change notification support in the MVVM pattern.
 *****************************************************************************/
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace FileCloner.ViewModels
{
    /// <summary>
    /// Base class implementing INotifyPropertyChanged to provide property change
    /// notification support to derived ViewModel classes.
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Event raised when a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Notifies the UI of changes to a property.
        /// </summary>
        /// <param name="property">Name of the property that changed.</param>
        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        /// <summary>
        /// Gets the dispatcher for the main application thread.
        /// If the application is in unit test mode, it returns the current thread's dispatcher.
        /// </summary>
        public static Dispatcher Dispatcher => Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
    }
}
