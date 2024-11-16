using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using UXModule;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Oauth2.v2;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using UXModule.ViewModel;

namespace UXModule.Views
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        private readonly string _userDataPath;
        private const string RedirectUri = "http://localhost:5041/signin-google";
        private static readonly string[] Scopes = { Oauth2Service.Scope.UserinfoProfile, Oauth2Service.Scope.UserinfoEmail };
        private readonly MainPageViewModel _viewModel;
        private string client_secret_path = Path.GetFullPath(Path.Combine("..", "..", "..", "Model", "client_secret.json"));

        public LoginPage(MainPageViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;

            // Store files in user's AppData folder
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Dashboard"
            );

            Directory.CreateDirectory(appDataPath);
            _userDataPath = Path.Combine(appDataPath, "user_data.json");
            //_userDataPath = "../../Models/Authentication/user_data.json";

            // Initialize StatusText
            StatusText = new TextBlock();
        }

        private async void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SignInButton.IsEnabled = false;

                if (File.Exists("token.json"))
                {
                    File.Delete("token.json");
                }
                if (File.Exists(_userDataPath))
                {
                    File.Delete(_userDataPath);
                }

                // Clear the stored credentials in the FileDataStore
                var credPath = "token.json";
                var fileDataStore = new FileDataStore(credPath, true);
                await fileDataStore.ClearAsync();


                var credential = await GetGoogleOAuthCredentialAsync();
                if (credential == null)
                {
                    MessageBox.Show("Failed to obtain credentials.");
                    return;
                }

                var userInfo = await GetUserInfoAsync(credential);
                if (userInfo == null)
                {
                    MessageBox.Show("Failed to obtain user information.");
                    return;
                }

                SaveUserInfoToFile(userInfo);

                // Navigate to HomePage and pass user info
                var homePage = new HomePage(_viewModel);
                homePage.SetUserInfo(userInfo.Name, userInfo.Email, userInfo.Picture);
                NavigationService.Navigate(homePage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sign-in error: {ex.Message}\n\nDetails: {ex.InnerException?.Message}");
            }
            finally
            {
                SignInButton.IsEnabled = true;
            }
        }

        private async void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Task.Run(() =>
                {
                    if (File.Exists("token.json"))
                    {
                        File.Delete("token.json");
                    }
                    if (File.Exists(_userDataPath))
                    {
                        File.Delete(_userDataPath);
                    }
                });

                // Clear the stored credentials in the FileDataStore
                var credPath = "token.json";
                var fileDataStore = new FileDataStore(credPath, true);
                await fileDataStore.ClearAsync();

                MessageBox.Show("Signed out successfully.");
                StatusText.Text = "Signed out. Please sign in again.";

                // Clear the displayed user information
                //ClearUserInfoDisplay();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sign-out error: {ex.Message}");
            }
        }

        private async Task<UserCredential?> GetGoogleOAuthCredentialAsync()
        {
            using (var stream = new FileStream(client_secret_path, FileMode.Open, FileAccess.Read))
            {
                var credPath = "token.json";
                var clientSecrets = GoogleClientSecrets.FromStream(stream).Secrets;
                return await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    clientSecrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true),
                    new LocalServerCodeReceiver(RedirectUri));
            }
        }

        private async Task<Userinfo?> GetUserInfoAsync(UserCredential credential)
        {
            var service = new Oauth2Service(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Google Sign-In WPF"
            });

            var userInfoRequest = service.Userinfo.Get();
            return await userInfoRequest.ExecuteAsync();
        }

        private void SaveUserInfoToFile(Userinfo userInfo)
        {
            var userData = new
            {
                Name = userInfo.Name,
                Email = userInfo.Email,
                Picture = userInfo.Picture,
                SavedAt = DateTime.UtcNow
            };

            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(userData, jsonOptions);
            File.WriteAllText(_userDataPath, jsonString);
        }

        //private void DisplayUserInfo(Userinfo userInfo)
        //{
        //    // Display the user info in a message box
        //    string formattedDisplay =
        //        $"Name: {userInfo.Name}\n" +
        //        $"Email: {userInfo.Email}\n" +
        //        $"Profile Picture URL: {userInfo.Picture}";

        //    MessageBox.Show(formattedDisplay, "User Info", MessageBoxButton.OK);

        //    // Display the user info in the application
        //    UserName.Text = userInfo.Name;
        //    EmailTextBlock.Text = userInfo.Email;
        //    ProfileImage.Source = new BitmapImage(new Uri(userInfo.Picture));
        //}


        //private void ClearUserInfoDisplay()
        //{
        //    // Clear the user info from the application
        //    NameTextBlock.Text = string.Empty;
        //    EmailTextBlock.Text = string.Empty;
        //    ProfileImage.Source = null;
        //}
        // Add the missing StatusText definition
        private TextBlock StatusText { get; set; }
    }
}
