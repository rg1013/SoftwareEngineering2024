/******************************************************************************
* Filename    = ServerViewModel.cs
*
* Author      = Garima Ranjan
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = ViewModel for Server side logic
*****************************************************************************/

using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Documents;
using Updater;

namespace ViewModels;
public class ServerViewModel : INotifyPropertyChanged
{
    private Server _server;
    private readonly LogServiceViewModel _logServiceViewModel;
    private readonly Mutex _mutex;
    private readonly ToolAssemblyLoader _loader;
    private static readonly JsonSerializerOptions s_jsonOptions = new() { WriteIndented = true };
    private bool _isRunning;

    public ServerViewModel(LogServiceViewModel logServiceViewModel, ToolAssemblyLoader loader)
    {
        _server = Server.GetServerInstance(AddLogMessage);
        _loader = loader;
        _logServiceViewModel = logServiceViewModel; 
        // Create a named mutex
        _mutex = new Mutex(false, "Global\\MyUniqueServerMutexName");
    }

    public Server GetServer()
    {
        return _server;
    }

    public bool CanStartServer()
    {
        return _mutex.WaitOne(0); // Check if the mutex can be acquired
    }

    public void StartServer(string ip, string port)
    {
        if (CanStartServer())
        {
            Task.Run(() => {
                _server.Start(ip, port);
                _isRunning = true;
            });
        }
        else
        {
            _logServiceViewModel.UpdateLogDetails("Server is already running on another instance.");
        }
    }

    public void StopServer()
    {
        _server.Stop();
        _mutex.ReleaseMutex();
        _isRunning = false;
    }

    private void AddLogMessage(string message)
    {
        _logServiceViewModel.UpdateLogDetails(message);
    }

    public bool IsServerRunning()
    {
        return _isRunning;
    }
    public string GetServerData()
    {
        string serverFolderPath = AppConstants.ToolsDirectory;
        var fileDataList = new List<object>();

        if (Directory.Exists(serverFolderPath))
        {
            try
            {
                // Load tool properties from the folder
                Dictionary<string, List<string>> toolProperties = _loader.LoadToolsFromFolder(serverFolderPath);

                // Extract properties for each file if keys exist
                Dictionary<string, List<string>> toolProperties1 = toolProperties;

                var fileData = new {
                    Id = toolProperties1.ContainsKey("Id") ? toolProperties["Id"] : new List<string> { "N/A" },
                    Name = toolProperties1.ContainsKey("Name") ? toolProperties["Name"] : new List<string> { "N/A" },
                    Description = toolProperties1.ContainsKey("Description") ? toolProperties["Description"] : new List<string> { "N/A" },
                    FileVersion = toolProperties1.ContainsKey("Version") ? toolProperties["Version"] : new List<string> { "N/A" },
                    LastUpdate = toolProperties1.ContainsKey("LastUpdated") ? toolProperties["LastUpdated"] : new List<string> { "N/A" },
                    LastModified = toolProperties1.ContainsKey("LastModified") ? toolProperties["LastModified"] : new List<string> { "N/A" },
                    CreatorName = toolProperties1.ContainsKey("CreatorName") ? toolProperties["CreatorName"] : new List<string> { "N/A" },
                    CreatorMail = toolProperties1.ContainsKey("CreatorEmail") ? toolProperties["CreatorEmail"] : new List<string> { "N/A" }
                };


                fileDataList.Add(fileData);
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error accessing directory {serverFolderPath}: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("Server directory not found.");
        }

        // Serialize the list of file data to JSON
        string jsonResult = JsonSerializer.Serialize(fileDataList, s_jsonOptions);
        return jsonResult;
    }

    /// <summary>
    /// Notify property changed
    /// </summary>
    /// <param name="propertyName">Property name</param>


    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
