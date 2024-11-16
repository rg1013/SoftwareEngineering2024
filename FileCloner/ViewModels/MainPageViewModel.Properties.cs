using FileCloner.Models.NetworkService;
using FileCloner.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FileCloner.ViewModels
{
    partial class MainPageViewModel : ViewModelBase
    {
        // Bindable property for the root directory path
        private string _rootDirectoryPath;
        public string RootDirectoryPath
        {
            get => _rootDirectoryPath;
            set
            {
                _rootDirectoryPath = value;
                OnPropertyChanged(nameof(RootDirectoryPath));
            }
        }

        private Node _selectedNode;
        public Node SelectedNode
        {
            get => _selectedNode;
            set
            {
                _selectedNode = value;
                OnPropertyChanged(nameof(SelectedNode));
            }
        }

        // Property to track the number of files in the directory
        private int _fileCount;
        public int FileCount
        {
            get => _fileCount;
            set
            {
                _fileCount = value;
                OnPropertyChanged(nameof(FileCount));
            }
        }

        // Property to track the number of folders in the directory
        private int _folderCount;
        public int FolderCount
        {
            get => _folderCount;
            set
            {
                _folderCount = value;
                OnPropertyChanged(nameof(FolderCount));
            }
        }

        // Property to track the count of selected folders in the UI
        private int _selectedFoldersCount;
        public int SelectedFoldersCount
        {
            get => _selectedFoldersCount;
            set
            {
                _selectedFoldersCount = value;
                OnPropertyChanged(nameof(SelectedFoldersCount));
            }
        }

        // Property to track the count of selected files in the UI
        private int _selectedFilesCount;
        public int SelectedFilesCount
        {
            get => _selectedFilesCount;
            set
            {
                _selectedFilesCount = value;
                OnPropertyChanged(nameof(SelectedFilesCount));
            }
        }

        // Property to track the total size of selected files in bytes
        private long _sumOfSelectedFilesSizeInBytes;
        public long SumofSelectedFilesSizeInBytes
        {
            get => _sumOfSelectedFilesSizeInBytes;
            set
            {
                _sumOfSelectedFilesSizeInBytes = value;
                OnPropertyChanged(nameof(SumofSelectedFilesSizeInBytes));
            }
        }

        private bool _isSendRequestEnabled;
        public bool IsSendRequestEnabled
        {
            get => _isSendRequestEnabled;
            set
            {
                _isSendRequestEnabled = value;
                OnPropertyChanged(nameof(IsSendRequestEnabled));
            }
        }
        private bool _isSummarizeEnabled;
        public bool IsSummarizeEnabled
        {
            get => _isSummarizeEnabled;
            set
            {
                _isSummarizeEnabled = value;
                OnPropertyChanged(nameof(IsSummarizeEnabled));
            }
        }
        private bool _isStartCloningEnabled;
        public bool IsStartCloningEnabled
        {
            get => _isStartCloningEnabled;
            set
            {
                _isStartCloningEnabled = value;
                OnPropertyChanged(nameof(IsStartCloningEnabled));
            }
        }
        private bool _isStopCloningEnabled;
        public bool IsStopCloningEnabled
        {
            get => _isStopCloningEnabled;
            set
            {
                _isStopCloningEnabled = value;
                OnPropertyChanged(nameof(IsStopCloningEnabled));
            }
        }

        // UI Commands for button actions
        public ICommand BrowseFoldersCommand { get; }
        public ICommand SendRequestCommand { get; }
        public ICommand SummarizeCommand { get; }
        public ICommand StartCloningCommand { get; }
        public ICommand StopCloningCommand { get; }

        // Collection to store log messages for display
        public ObservableCollection<string> LogMessages { get; set; } = new ObservableCollection<string>();

        // Tree structure of nodes representing files and directories
        public ObservableCollection<Node> Tree { get; set; }
        private readonly FileExplorerServiceProvider _fileExplorerServiceProvider;

        // Dictionary to store selected files, with address as key and list of file paths as value
        public static Dictionary<string, List<string>> _selectedFiles = new();

        // Client and Server instances for network communication
        private readonly Client _client;
        private readonly Server _server;

        // Singleton instance (assuming this pattern is suitable for your use case)
        private static MainPageViewModel _instance;
    }
}
