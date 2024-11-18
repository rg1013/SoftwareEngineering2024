/****************************************************************************** 
 * Filename    = UpdaterPage.xaml.cs 
 * 
 * Author      = Updater Team 
 * 
 * Product     = UI 
 * 
 * Project     = Views 
 * 
 * Description = Initialize a page for Updater 
 *****************************************************************************/

using System.Windows;
using System.Windows.Controls;
using Updater;
using ViewModels;

namespace UI.Views
{
    /// <summary> 
    /// Interaction logic for UpdaterPage.xaml 
    /// </summary> 
    public partial class UpdaterPage : Page
    {
        public LogServiceViewModel LogServiceViewModel { get; }
        private FileChangeNotifier _analyzerNotificationService;
        private ToolListViewModel _toolListViewModel;
        private ServerViewModel _serverViewModel; // Added server view model 
        private ClientViewModel _clientViewModel; // Added client view model 

        public UpdaterPage()
        {
            InitializeComponent();
            _toolListViewModel = new ToolListViewModel();
            _toolListViewModel.LoadAvailableTools();
            ListView listView = (ListView)this.FindName("ToolViewList");
            listView.DataContext = _toolListViewModel;

            _analyzerNotificationService = new FileChangeNotifier();
            _analyzerNotificationService.MessageReceived += OnMessageReceived;

            LogServiceViewModel = new LogServiceViewModel();
            DataContext = LogServiceViewModel;

            _serverViewModel = new ServerViewModel(LogServiceViewModel); // Initialize the server view model 
            _clientViewModel = new ClientViewModel(LogServiceViewModel); // Initialize the client view model 
        }

        private void OnMessageReceived(string message)
        {
            _toolListViewModel.LoadAvailableTools(); // Refresh the tool list on message receipt 
            LogServiceViewModel.ShowNotification(message); // Show received message as a notification 
            LogServiceViewModel.UpdateLogDetails(message); // Update log with received message 
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _toolListViewModel.LoadAvailableTools(); // Load tools when the page loads 
            LogServiceViewModel.UpdateLogDetails("Page loaded successfully.\n");
            LogServiceViewModel.ShowNotification("Welcome to the application!"); // Welcome notification 
        }

        private void StartServerButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if the server can be started 
            if (_serverViewModel.CanStartServer())
            {
                string ip = AppConstants.ServerIP; // Assume you have an IpTextBox for IP input 
                string port = AppConstants.Port; // Assume you have a PortTextBox for Port input 

                _serverViewModel.StartServer(ip, port); // Call to start the server 

                StartServerButton.IsEnabled = false; // Disable Start button 
                StopServerButton.IsEnabled = true; // Enable Stop button 

                // Disable all other buttons
                DisconnectButton.IsEnabled = false; // Disable Disconnect button
                ConnectButton.IsEnabled = false; // Disable Connect button
                                                 // Add any additional buttons you want to disable here
            }
            else
            {
                LogServiceViewModel.UpdateLogDetails("Server is already running on another instance.\n");
            }
        }

        private void StopServerButton_Click(object sender, RoutedEventArgs e) // Handler for stop button click 
        {
            _serverViewModel.StopServer(); // Call to stop the server 

            // Enable the Start Server button after stopping the server 
            StartServerButton.IsEnabled = true;
            StopServerButton.IsEnabled = false; // Disable Stop button 
                                                // Disable all other buttons
            DisconnectButton.IsEnabled = true; // Disable Disconnect button
            ConnectButton.IsEnabled = true; // Disable Connect button

        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e) // Handler for connect button click 
        {
            if (_clientViewModel.CanConnect)
            {
                LogServiceViewModel.UpdateLogDetails("Connecting to server...\n"); // Log connecting message
                await _clientViewModel.ConnectAsync();

                // Disable all other buttons except for Disconnect
                StartServerButton.IsEnabled = false; // Disable Start button
                StopServerButton.IsEnabled = false; // Disable Stop button
                ConnectButton.IsEnabled = false; // Disable Connect button

                // Enable Disconnect button
                if (_clientViewModel.IsConnected)
                {
                    LogServiceViewModel.UpdateLogDetails("Successfully connected to server!\n"); // Log successful connection
                    DisconnectButton.IsEnabled = true; // Enable Disconnect button
                }
                else
                {
                    LogServiceViewModel.UpdateLogDetails("Failed to connect to server.\n"); // Log failure
                    StartServerButton.IsEnabled = true; // Disable Start button
                    StopServerButton.IsEnabled = false; // Disable Stop button
                    ConnectButton.IsEnabled = true;
                }
            }
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e) // Handler for disconnect button click 
        {
            if (_clientViewModel.CanDisconnect)
            {
                _clientViewModel.Disconnect();
                LogServiceViewModel.UpdateLogDetails("Disconnected from server.\n"); // Log disconnection message

                // Enable buttons after disconnection
                StartServerButton.IsEnabled = true; // Enable Start button
                StopServerButton.IsEnabled = false; // Disable Stop button
                ConnectButton.IsEnabled = true; // Enable Connect button
                DisconnectButton.IsEnabled = false; // Disable Disconnect button
            }
        }
    }
}
