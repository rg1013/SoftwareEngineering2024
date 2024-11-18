/******************************************************************************
* Filename    = ServerViewModel.cs
*
* Author      = Garima Ranjan and Karumudi Harika
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = ViewModel for Server side logic
*****************************************************************************/

using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using Updater;

namespace ViewModels.Updater;

/// <summary>
/// ViewModel responsible for server-related functionality, including 
/// broadcasting files and retrieving tool data stored on the server.
/// </summary>
public class ServerViewModel : INotifyPropertyChanged
{
    private readonly Server _server;
    private readonly LogServiceViewModel _logServiceViewModel;
    private readonly ToolAssemblyLoader _loader;
    private static readonly JsonSerializerOptions s_jsonOptions = new() { WriteIndented = true };

    /// <summary>
    /// Constructor initializes the server instance, log service, and tool loader.
    /// </summary>
    /// <param name="logServiceViewModel">Log service for handling log updates.</param>
    /// <param name="loader">Tool loader for managing tools in the server directory.</param>
    /// <param name="server">Optional server instance; defaults to a singleton instance.</param>
    public ServerViewModel(LogServiceViewModel logServiceViewModel, ToolAssemblyLoader loader, Server? server = null)
    {
        server = Server.GetServerInstance(AddLogMessage);
        _server = server;
        _loader = loader;
        _logServiceViewModel = logServiceViewModel;
    }

    /// <summary>
    /// Retrieves the server instance associated with the ViewModel.
    /// </summary>
    /// <returns>The current server instance.</returns>
    public Server GetServer()
    {
        return _server;
    }

    /// <summary>
    /// Adds a log message to the log service ViewModel.
    /// </summary>
    /// <param name="message">The log message to add.</param>
    private void AddLogMessage(string message)
    {
        _logServiceViewModel.UpdateLogDetails(message);
    }
    /// <summary>
    /// Retrieves metadata about tools stored in the server directory and returns it as a JSON string.
    /// </summary>
    /// <returns>JSON-formatted string containing tool metadata.</returns>
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
    /// Broadcasts a file to all connected clients.
    /// </summary>
    /// <param name="filePath">Path of the file to be broadcasted.</param>
    /// <param name="fileName">Name of the file to be broadcasted.</param>
    public void BroadcastToClients(string filePath, string fileName)
    {
        string? content = Utils.ReadBinaryFile(filePath) ?? throw new Exception("Failed to read file");
        string? serializedContent = Utils.SerializeObject(content) ?? throw new Exception("Failed to serialize content");
        var fileContentToSend = new FileContent(fileName, serializedContent);

        _logServiceViewModel.UpdateLogDetails("Sending files to all connected clients");

        var fileContentsToSend = new List<FileContent>();
        fileContentsToSend?.Add(fileContentToSend);

        // Create data packet to send
        var dataPacketToSend = new DataPacket(
                        DataPacket.PacketType.Broadcast,
                        new List<FileContent> { fileContentToSend }
                        );

        // Set the target directory where files will be saved
        string targetDirectory = AppConstants.ToolsDirectory;

        // Create the target file path
        string targetFilePath = Path.Combine(targetDirectory, Path.GetFileName(filePath));
        try
        {
            // Copy the file to the target directory
            File.Copy(filePath, targetFilePath, overwrite: true);
            // Notify the user and log success
            _logServiceViewModel.UpdateLogDetails($"File uploaded successfully: {Path.GetFileName(filePath)}");
            MessageBox.Show("File uploaded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            // Handle errors during file copy
            _logServiceViewModel.UpdateLogDetails($"Failed to upload file: {ex.Message}");
            MessageBox.Show($"Error uploading file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        // Broadcast the serialized packet
        string serializedPacket = Utils.SerializeObject(dataPacketToSend);
        _server.Broadcast(serializedPacket);
    }

    /// <summary>
    /// Notify property changed
    /// </summary>
    /// <param name="propertyName">Property name</param>


    public event PropertyChangedEventHandler? PropertyChanged;
    /// <summary>
    /// Triggers the PropertyChanged event for a specific property.
    /// </summary>
    /// <param name="propertyName">Name of the property that changed.</param>

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
