using System.ComponentModel;
using System.Windows.Input;
using UI.Helpers;

namespace UI.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private string? _message;
        public DashboardViewModel()
        {
            // Initialize the command for button clicks
            ButtonClickCommand = new RelayCommand(OnButtonClick);
        }
        // Property to display a message when a button is clicked
        public string Message
        {
            get { return _message; }
            set
            {
                if (_message != value)
                {
                    _message = value;
                    OnPropertyChanged(nameof(Message));
                }
            }
        }

        // Command for handling button clicks
        public ICommand ButtonClickCommand { get; private set; }

        // Method that handles button clicks
        private void OnButtonClick(object parameter)
        {
            if (parameter is string buttonName)
            {
                Message = $"{buttonName} clicked!";
            }
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
