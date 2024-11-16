/******************************************************************************
 * Filename    = MainPageViewModel.cs
 *
 * Author      = Sai Hemanth Reddy
 * 
 * Project     = FileCloner
 *
 * Description = ViewModel for MainPage, handling directory loading, file structure 
 *               generation, and managing file/folder selection states. Implements 
 *               commands for sending requests, summarizing responses, and starting 
 *               file cloning.
 *****************************************************************************/

using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Diagnostics;
using System.Windows.Input;
using FileCloner.Models;
using Microsoft.WindowsAPICodePack.Dialogs;
using FileCloner.Models.NetworkService;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;

namespace FileCloner.ViewModels
{
    /// <summary>
    /// ViewModel for the MainPage, handling file operations, commands, and UI bindings.
    /// </summary>
    partial class MainPageViewModel : ViewModelBase
    {
        /// <summary>
        /// Constructor for MainPageViewModel - initializes paths, commands, and communication setup.
        /// </summary>
        public MainPageViewModel()
        {
            _instance = this; // Set instance for static access

            if (!Directory.Exists(Constants.defaultFolderPath))
            {
                Directory.CreateDirectory(Constants.defaultFolderPath);
            }
            // Set default root directory path
            RootDirectoryPath = Constants.defaultFolderPath;

            // Initialize FileExplorerServiceProvider to manage file operations
            _fileExplorerServiceProvider = new FileExplorerServiceProvider();

            // Initialize UI commands and their respective handlers
            SendRequestCommand = new RelayCommand(SendRequest);
            SummarizeCommand = new RelayCommand(SummarizeResponses);
            StartCloningCommand = new RelayCommand(StartCloning);
            BrowseFoldersCommand = new RelayCommand(BrowseFolders);
            StopCloningCommand = new RelayCommand(StopCloning);

            //For watching files and updating any changes in the UI accordingly
            Thread fileWatcherThread = new(() => WatchFile(RootDirectoryPath));
            fileWatcherThread.Start();

            //Only SendRequest button will be enabled in the beginning
            IsSendRequestEnabled = true;
            IsSummarizeEnabled = false;
            IsStartCloningEnabled = false;
            IsStopCloningEnabled = false;

            // Subscribe to CheckBoxClickEvent to update selection counts when a checkbox is clicked
            Node.CheckBoxClickEvent += UpdateCounts;

            // Initialize the Tree structure representing files and folders
            Tree = new ObservableCollection<Node>();
            TreeGenerator(_rootDirectoryPath);  // Load the initial tree structure

            // Initialize server and client for handling file transfer communication
            _server = Server.GetServerInstance(UpdateLog);
            _client = new Client(UpdateLog);

            // Register for application exit event to ensure resources are released
            System.Windows.Application.Current.Exit += (sender, e) =>
            {
                _client.Stop();
                _server.Stop();
            };
        }
    }
}
