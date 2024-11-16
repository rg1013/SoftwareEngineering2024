using System;
using System.Collections.Generic;
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

namespace UXModule.Views
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        private readonly MainPageViewModel _viewModel;

        public HomePage(MainPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            this.DataContext = _viewModel;
        }

        public void SetUserInfo(string userName, string userEmail, string userProfilePictureUrl)
        {
            UserName.Text = userName;
            UserEmail.Text = userEmail;
            UserProfilePicture.Source = new BitmapImage(new Uri(userProfilePictureUrl));
        }

        private void CreateSession_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string username = UserName.Text;
                string useremail = UserEmail.Text;
                string profilePictureUrl = UserProfilePicture.Source.ToString();
                string? response = _viewModel.CreateSession(username, useremail, profilePictureUrl);
                if (response == "success")
                {
                    var serverHomePage = new ServerHomePage(_viewModel);
                    this.NavigationService.Navigate(new MainPage("server",_viewModel,serverHomePage));
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void JoinSession_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_viewModel != null)
                {
                    string username = UserName.Text;
                    string useremail = UserEmail.Text;
                    string serverip = ServerIP.Text;
                    string serverport = ServerPort.Text;
                    string profilePictureUrl = UserProfilePicture.Source.ToString();

                    // Validate Server IP and Server Port
                    if (string.IsNullOrWhiteSpace(serverip))
                    {
                        MessageBox.Show("Server IP cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(serverport) || !int.TryParse(serverport, out _))
                    {
                        MessageBox.Show("Server Port must be a valid number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string response = _viewModel.JoinSession(username, useremail, serverip, serverport, profilePictureUrl);
                    if (response == "success")
                    {
                        var clientHomePage = new ClientHomePage(_viewModel);
                        this.NavigationService.Navigate(new MainPage("client",_viewModel,clientHomePage));
                    }
                    else
                    {
                        MessageBox.Show("Failed to join session.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
