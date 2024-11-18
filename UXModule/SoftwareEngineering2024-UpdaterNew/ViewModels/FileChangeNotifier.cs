/******************************************************************************
* Filename    = FileChangeNotifier.cs
*
* Author      = Karumudi Harika
*
* Product     = Updater.Client
* 
* Project     = File Watcher
*
* Description = Notifies if any new analyzer file(dll) either added or deleted to the watching folder.
*****************************************************************************/

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.IO;
using Updater;

namespace ViewModels;

/// <summary>
/// The FileMonitor class monitors a specified folder for file creation and deletion events
/// and raises property changes to update the UI with status messages.
/// </summary>
public class FileChangeNotifier : INotifyPropertyChanged
{
    //Holds the current status message
    private string? _messageStatus;
    //Monitors the file system changes in the directory
    private FileSystemWatcher? _fileWatcher;
    //Stores list of created files
    private List<string>? _createdFiles;
    //Stores the list of deleted files
    private List<string>? _deletedFiles;
    //Timer to debounce file change events for batch processing
    private Timer? _timer;
    public event Action<string>? MessageReceived;

    /// <summary>
    /// Initializes a new instance of the FileChangeNotifier class and starts monitoring the folder.
    /// </summary>
    public FileChangeNotifier()
    {
        //Intialize the list for created and deleted files.
        _createdFiles = new List<string>();
        _deletedFiles = new List<string>();
        StartMonitoring();
    }

    /// <summary>
    /// Gets or sets the current status message of the file monitoring process.
    /// This message is updated when files are created or deleted.
    /// </summary>
    public string MessageStatus
    {
        //Returns the current message status
        get => _messageStatus;
        set
        {
            //updates the message status and notifies the UI about the status change.
            _messageStatus = value;
            OnPropertyChanged(nameof(MessageStatus));
        }
    }

    /// <summary>
    /// Starts monitoring the specified folder for file creation and deletion events.
    /// </summary>
    private void StartMonitoring()
    {
        //Path to folder to monitor
        string folderPath = AppConstants.ToolsDirectory;

        //Check if folder exists, if not, create it.
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            MessageStatus = $"Created folder: {folderPath}";
        }
        _fileWatcher = new FileSystemWatcher
        {
            Path = folderPath,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName,
            Filter = "*.*"
        };

        _fileWatcher.Created += OnFileCreated;
        _fileWatcher.Deleted += OnFileDeleted;
        _fileWatcher.EnableRaisingEvents = true;

        MessageStatus = $"Monitoring folder: {folderPath}";

        //Initialize timer with 1 second interval (adjust as necessary)
        _timer = new Timer(OnTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);
    }

    /// <summary>
    /// Event handler for the Created event of the FileSystemWatcher.
    /// Add the created file path to a list and triggers the timer for processing.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A FileSystemEventArgs thta contains the event data.</param>
    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        // Add the file to the list
        lock (_createdFiles)
        {
            _createdFiles.Add(e.FullPath);
        }

        // Restart the timer for debouncing
        _timer.Change(1000, Timeout.Infinite); // 1 second delay before processing (adjust as needed)
    }


    /// <summary>
    /// Event handler for the Deleted event of the FileSystemWatcher.
    /// Adds the deleted file path to a list and triggers the timer for processing.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A FileSystemEventArgs that contains the event data.</param>

    private void OnFileDeleted(object sender, FileSystemEventArgs e)
    {
        // Add the file to the list
        lock (_deletedFiles)
        {
            _deletedFiles.Add(e.FullPath);
        }

        // Restart the timer for debouncing
        _timer.Change(1000, Timeout.Infinite); // 1 second delay before processing (adjust as needed)
    }

    /// <summary>
    /// Timer callback method that processes the lists of created and deleted files
    /// and updates the MessageStatus property with the appropriate messages.
    /// </summary>
    /// <param name="state">An object containing information about the timer event.</param>
    private void OnTimerElapsed(object state)
    {
        List<string> filesToProcess;

        // Lock and extract the current batch of files
        lock (_createdFiles)
        {
            filesToProcess = new List<string>(_createdFiles);
            _createdFiles.Clear();
        }

        List<string> deletedFilesToProcess;

        lock (_deletedFiles)
        {
            deletedFilesToProcess = new List<string>(_deletedFiles);
            _deletedFiles.Clear();
        }


        StringBuilder message = new StringBuilder();

        if (filesToProcess.Any())
        {
            string fileList = string.Join(", ", filesToProcess.Select(Path.GetFileName));
            message.AppendLine($"Files created: {fileList}");
        }

        if (deletedFilesToProcess.Any())
        {
            string deletedFileList = string.Join(", ", deletedFilesToProcess.Select(Path.GetFileName));
            message.AppendLine($"Files removed: {deletedFileList}");
        }
        if (message.Length > 0)
        {
            string v = message.ToString();
            MessageStatus = v;
            MessageReceived?.Invoke(v);
        }
    }

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
