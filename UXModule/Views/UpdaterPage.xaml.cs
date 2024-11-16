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

namespace UXModule.Views;


/// <summary> 
/// Interaction logic for UpdaterPage.xaml 
/// </summary> 
public partial class UpdaterPage : Page
{
    public LogServiceViewModel LogServiceViewModel { get; }
    private readonly FileChangeNotifier _analyzerNotificationService;
    private readonly ToolListViewModel _toolListViewModel;
    private static CloudViewModel _cloudViewModel;
    private static ServerViewModel _serverViewModel; // Added server view model 
    private static ClientViewModel _clientViewModel; // Added client view model 
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
            _clientViewModel = new ClientViewModel(LogServiceViewModel); // Initialize the client view model 
        }
        else
        {
            _serverViewModel = new ServerViewModel(LogServiceViewModel, _loader); // Initialize the server view model 
            _cloudViewModel = new CloudViewModel(LogServiceViewModel, _serverViewModel);
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
        if (_sessionType != "server")
        {
            try
            {
                LogServiceViewModel.UpdateLogDetails("Initiating sync with the server...\n");
                await _clientViewModel.SyncUpAsync(); // Call the sync method on the ViewModel
            }
            catch (Exception ex)
            {
                LogServiceViewModel.UpdateLogDetails("Client is not connected. Please connect first.\n");
            }
        }
    }

    private async void SyncCloudButtonClick(object sender, RoutedEventArgs e)
    {
        // Disable the Sync button to prevent multiple syncs at the same time
        CloudSyncButton.IsEnabled = false;
        if (_sessionType=="server")
        {
            try
            {
                // Check if the server is running
                if (!_serverViewModel.IsServerRunning())
                {
                    LogServiceViewModel.UpdateLogDetails("Cloud sync aborted. Please start the server first.");
                    return;
                }

                LogServiceViewModel.UpdateLogDetails("Server is running. Starting cloud sync.");

                // Perform cloud sync asynchronously
                await _cloudViewModel.PerformCloudSync();

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
