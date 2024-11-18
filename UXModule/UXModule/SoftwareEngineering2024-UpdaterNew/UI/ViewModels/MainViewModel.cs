using System.ComponentModel;
using System.Windows;
using UI.Views;

namespace UI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private object _dashboardPage;
        private object _whiteboardPage;
        private object _analyzerPage;
        private object _screensharePage;
        private object _fileclonerPage;
        private object _updaterPage;
        private object _contentPage;

        private int _selectedTabIndex;

        public MainViewModel()
        {
            // Initialize pages
            DashboardPage = new DashboardPage();
            WhiteboardPage = new WhiteboardPage();
            AnalyzerPage = new AnalyzerPage();
            ScreensharePage = new ScreensharePage();
            FileClonerPage = new FileClonerPage();
            UpdaterPage = new UpdaterPage();
            ContentPage = new ContentPage();


            // Default to the first tab (Dashboard)
            SelectedTabIndex = 0;
        }

        public object DashboardPage
        {
            get { return _dashboardPage; }
            set
            {
                _dashboardPage = value;
                OnPropertyChanged(nameof(DashboardPage));
            }
        }

        public object FileClonerPage
        {
            get { return _fileclonerPage; }
            set
            {
                _fileclonerPage = value;
                OnPropertyChanged(nameof(FileClonerPage));
            }
        }

        public object ContentPage
        {
            get { return _contentPage; }
            set
            {
                _contentPage = value;
                OnPropertyChanged(nameof(ContentPage));
            }
        }

        public object UpdaterPage
        {
            get { return _updaterPage; }
            set
            {
                _updaterPage = value;
                OnPropertyChanged(nameof(UpdaterPage));
            }
        }

        public object WhiteboardPage
        {
            get { return _whiteboardPage; }
            set
            {
                _whiteboardPage = value;
                OnPropertyChanged(nameof(WhiteboardPage));
            }
        }

        public object AnalyzerPage
        {
            get { return _analyzerPage; }
            set
            {
                _analyzerPage = value;
                OnPropertyChanged(nameof(AnalyzerPage));
            }
        }

        public object ScreensharePage
        {
            get { return _screensharePage; }
            set
            {
                _screensharePage = value;
                OnPropertyChanged(nameof(ScreensharePage));
            }
        }

        // Index of the currently selected tab
        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set
            {
                _selectedTabIndex = value;
                OnPropertyChanged(nameof(SelectedTabIndex));
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
