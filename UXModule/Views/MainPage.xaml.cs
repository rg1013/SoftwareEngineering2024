using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UXModule.ViewModel;
using FileCloner.Views;

namespace UXModule.Views
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {

        private static DashboardPage dashboardPage;
        //private static WhiteboardGUI.Views.MainPage whiteboardPage;
        //private static UpdaterPage updaterPage;
        private static FileCloner.Views.MainPage fileClonerPage;
        //private static ScreensharePage screensharePage;
        //private static AnalyserPage analyserPage;
        //private static ChatPage chatPage;
        //private static UploadPage uploadPage;

        private readonly string sessionType;
        private static Page _currentPage;

        public event PropertyChangingEventHandler? PropertyChanged;

        public MainPage(string _sessionType, MainPageViewModel mainPageViewModel, Page currentPage)
        {
            InitializeComponent();
            sessionType = _sessionType;
            dashboardPage = new DashboardPage();
            _currentPage = currentPage; 

            Main.Content = dashboardPage;
           
        }

        private void DashboardClick(object sender, RoutedEventArgs e)
        {
            if (sessionType == "server")
            {
                Main.Content = _currentPage;
            }
            else
            {
                Main.Content = _currentPage;
            }
        }

        private void WhiteboardClick(object sender, RoutedEventArgs e)
        {
            //whiteboardPage = new WhiteboardGUI.Views.MainPage();
            //Main.Content = whiteboardPage;
        }


        private void FileClonerClick(object sender, RoutedEventArgs e)
        {
            fileClonerPage = new FileCloner.Views.MainPage();
            Main.Content = fileClonerPage;

        }

        private void UpdaterClick(object sender, RoutedEventArgs e)
        {

            //updaterPage = new UpdaterPage();
            //Main.Content = updaterPage;

        }

        private void AnalyserClick(object sender, RoutedEventArgs e)
        {
            //analyserPage = new AnalyserPage();
            //Main.Content = analyserPage;

        }

        private void ScreenShareClick(object sender, RoutedEventArgs e)
        {
            //if(sessionType == "server")
            //{
            //    Main.Content = new ScreenShareServer();
            //}
            //else
            //{
            //    Main.Content = new ScreenShareClient();
            //}

        }

        private void ChatButtonClick(object sender, RoutedEventArgs e)
        {
            //chatPage = new ChatPage();
            //Main.Content = chatPage;

        }

        private void UploadClick(object sender, RoutedEventArgs e)
        {
            //uploadPage = new UploadPage();
            //Main.Content = uploadPage;

        }



    }
}
