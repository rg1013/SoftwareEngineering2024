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

using System.IO;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using Updater;
using ViewModels.Updater;

namespace UXModule.Views;


/// <summary> 
/// Interaction logic for UpdaterPage.xaml 
/// </summary> 
public partial class UpdaterPage : Page
{
    public LogServiceViewModel LogServiceViewModel { get; }
    private readonly FileChangeNotifier _analyzerNotificationService;
    private readonly ToolListViewModel _toolListViewModel;
    private static CloudViewModel? s_cloudViewModel;
    private static ServerViewModel? s_serverViewModel; // Added server view model 
    private static ClientViewModel? s_clientViewModel; // Added client view model 
    private readonly ToolAssemblyLoader _loader;

    private readonly string _sessionType;
    public UpdaterPage(string sessionType)
    {
        InitializeComponent();

        _toolListViewModel = new ToolListViewModel();
        _toolListViewModel.LoadAvailableTools();

        ListView listView = (ListView)this.FindName("ToolViewList");
        listView.DataContext = _toolListViewModel;

        _analyzerNotificationService = new FileChangeNotifier();
        _analyzerNotificationService.MessageReceived += OnMessageReceived;

        LogServiceViewModel = new LogServiceViewModel();
        _loader = new ToolAssemblyLoader();

        _sessionType = sessionType;



        if (sessionType != "server")
        {
            s_clientViewModel = new ClientViewModel(LogServiceViewModel); // Initialize the client view model 
        }
        else
        {
            s_serverViewModel = new ServerViewModel(LogServiceViewModel, _loader); // Initialize the server view model 
            s_cloudViewModel = new CloudViewModel(LogServiceViewModel, s_serverViewModel);
        }

        this.DataContext = LogServiceViewModel;

    }

    private void OnMessageReceived(string message)
    {
        _toolListViewModel.LoadAvailableTools(); // Refresh the tool list on message receipt 
        LogServiceViewModel.ShowNotification(message); // Show received message as a notification 
        LogServiceViewModel.UpdateLogDetails(message); // Update log with received message 
    }

    private async void SyncButtonClick(object sender, RoutedEventArgs e)
    {
        if (s_clientViewModel != null)
        {
            if (_sessionType != "server")
            {
                try
                {
                    LogServiceViewModel.UpdateLogDetails("Initiating sync with the server...\n");
                    await s_clientViewModel.SyncUpAsync(); // Call the sync method on the ViewModel
                }
                catch (Exception)
                {
                    LogServiceViewModel.UpdateLogDetails("Client is not connected. Please connect first.\n");
                }
            }
        }
        else
        {
            // Open File Dialog
            OpenFileDialog openFileDialog = new OpenFileDialog {
                Title = "Select a File to Upload",
                Filter = "All Files (*.*)|*.*",
                Multiselect = false // Allow selecting only one file
            };

            if (s_serverViewModel != null)
            {
                if (openFileDialog.ShowDialog() == true)
                {
                    string selectedFilePath = openFileDialog.FileName; // Get the selected file path
                    string selectedFileName = Path.GetFileName(selectedFilePath); // Extract the file name from the path

                    s_serverViewModel.BroadcastToClients(selectedFilePath, selectedFileName);
                }
            }
        }
    }

    private async void SyncCloudButtonClick(object sender, RoutedEventArgs e)
    {
        // Disable the Sync button to prevent multiple syncs at the same time
        CloudSyncButton.IsEnabled = false;
        if (s_cloudViewModel != null)
        {
            if (_sessionType == "server")
            {
                try
                {
                    LogServiceViewModel.UpdateLogDetails("Server is running. Starting cloud sync.");

                    // Perform cloud sync asynchronously
                    await s_cloudViewModel.PerformCloudSync();

                    LogServiceViewModel.UpdateLogDetails("Cloud sync completed.");
                }
                catch (Exception ex)
                {
                    LogServiceViewModel.UpdateLogDetails($"Error during cloud sync: {ex.Message}");
                }
                finally
                {
                    CloudSyncButton.IsEnabled = true; // Re-enable Sync button
                }
            }
        }
    }

    private void ClosePopup(object sender, RoutedEventArgs e)
    {
        LogServiceViewModel.NotificationVisible = false;
    }
}
