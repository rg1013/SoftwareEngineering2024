/******************************************************************************
* Filename    = Client.cs
*
* Author      = Amithabh A and Garima Ranjan
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = Client side sending and receiving files logic
*****************************************************************************/

using Networking.Communication;
using Networking;
using System.Diagnostics;

namespace Updater;

public class Client : INotificationHandler
{
    private readonly ICommunicator _communicator;
    private static readonly string s_clientDirectory = AppConstants.ToolsDirectory;
    private static Client? s_instance;
    private static readonly object s_lock = new object();
    private string? _clientId;

    private Client()
    {
        _communicator = CommunicationFactory.GetCommunicator(isClientSide: true);
        _communicator.Subscribe("FileTransferHandler", this);
    }

    public void GetClientId(string clientId)
    {
        _clientId = clientId;
    }
    public static Client GetClientInstance(Action<string>? notificationReceived = null)
    {
        lock (s_lock)
        {
            if (s_instance == null)
            {
                s_instance = new Client();
            }

            if (notificationReceived != null)
            {
                OnLogUpdate = notificationReceived;
            }
        }
        return s_instance;
    }

    /// <summary>
    /// Sends a SyncUp request to the server
    /// </summary>
    public void SyncUp()
    {
        try
        {
            string serializedSyncUpPacket = Utils.SerializedSyncUpPacket(clientId: _clientId) ?? throw new Exception("Failed to serialize SyncUpPacket");

            UpdateUILogs("Sending syncup request to the server");
            Trace.WriteLine("[Updater] Sending data as FileTransferHandler from Manual Sync up...");
            _communicator.Send(serializedSyncUpPacket, "FileTransferHandler", null);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[Updater] Error in SyncUp: {ex.Message}");
        }
    }

    public void Stop()
    {
        UpdateUILogs("Client disconnected");
        _communicator.Stop();
    }


    public static void ShowInvalidFilesInUI(List<string> invalidFiles)
    {
        UpdateUILogs("Invalid filenames: Please change the name of the following files and manually sync up again");
        foreach (string file in invalidFiles)
        {
            UpdateUILogs(file);
        }
        UpdateUILogs("Sync up failed");
    }

    public static void PacketDemultiplexer(string serializedData, ICommunicator communicator)
    {
        try
        {
            DataPacket dataPacket = Utils.DeserializeObject<DataPacket>(serializedData);

            // Check PacketType
            switch (dataPacket.DataPacketType)
            {
                case DataPacket.PacketType.SyncUp:
                    SyncUpHandler(dataPacket, communicator);
                    break;
                case DataPacket.PacketType.InvalidSync:
                    InvalidSyncHandler(dataPacket, communicator);
                    break;
                case DataPacket.PacketType.Broadcast:
                    BroadcastHandler(dataPacket, communicator);
                    break;
                case DataPacket.PacketType.Differences:
                    DifferencesHandler(dataPacket, communicator);
                    break;
                default:
                    UpdateUILogs("Something went wrong! Please try manual sync up after some time");
                    throw new Exception("[Updater] Invalid PacketType");
            }
        }
        catch (Exception ex)
        {
            UpdateUILogs("Something went wrong! Please try manual sync up after some time");
            Trace.WriteLine($"[Updater] Error in PacketDemultiplexer: {ex.Message}");
        }
    }

    private static void SyncUpHandler(DataPacket dataPacket, ICommunicator communicator)
    {
        try
        {
            UpdateUILogs("Received SyncUp request from server");
            string serializedMetaData = Utils.SerializedMetadataPacket() ?? throw new Exception("Failed to serialize metadata");

            // Sending data to server
            Trace.WriteLine("[Updater] Sending data as FileTransferHandler...");
            communicator.Send(serializedMetaData, "FileTransferHandler", null);

            UpdateUILogs("Metadata sent to server");
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Error in SyncUpHandler: {ex.Message}");
        }
    }

    private static void InvalidSyncHandler(DataPacket dataPacket, ICommunicator communicator)
    {
        try
        {
            FileContent fileContent = dataPacket.FileContentList[0];
            if (fileContent.SerializedContent == null)
            {
                throw new Exception("SerializedContent in FileContent is null");
            }
            string serializedContent = fileContent.SerializedContent;
            List<string> invalidFileNames = Utils.DeserializeObject<List<string>>(serializedContent);
            UpdateUILogs("Received invalid file names from server");
            ShowInvalidFilesInUI(invalidFileNames);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Error in InvalidSyncHandler: {ex.Message}");
        }
    }

    private static void BroadcastHandler(DataPacket dataPacket, ICommunicator communicator)
    {
        try
        {
            UpdateUILogs("Recieved Broadcast from server");
            // File list
            List<FileContent> fileContentList = dataPacket.FileContentList;

            // Get files
            foreach (FileContent fileContent in fileContentList)
            {
                if (fileContent != null && fileContent.SerializedContent != null && fileContent.FileName != null)
                {
                    // Deserialize the content based on expected format
                    string content = Utils.DeserializeObject<string>(fileContent.SerializedContent);
                    string filePath = Path.Combine(s_clientDirectory, fileContent.FileName);
                    bool status = Utils.WriteToFileFromBinary(filePath, content);
                    if (!status)
                    {
                        throw new Exception("Failed to write file");
                    }
                }
            }
            UpdateUILogs("Up-to-date with the server");
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[Updater] Error in BroadcastHandler: {ex.Message}");
        }
    }

    private static void DifferencesHandler(DataPacket dataPacket, ICommunicator communicator)
    {
        try
        {
            UpdateUILogs("Recieved files from Server");
            List<FileContent> fileContentList = dataPacket.FileContentList;

            // Deserialize the 'differences' file content
            FileContent differenceFile = fileContentList[0];
            string? serializedDifferences = differenceFile.SerializedContent;
            string? differenceFileName = differenceFile.FileName;

            if (serializedDifferences == null)
            {
                throw new Exception("[Updater] SerializedContent is null");
            }

            // Deserialize to List<MetadataDifference>
            List<MetadataDifference> differencesList = Utils.DeserializeObject<List<MetadataDifference>>(serializedDifferences);

            // Process additional files in the list
            foreach (FileContent fileContent in fileContentList)
            {
                if (fileContent == differenceFile)
                {
                    continue;
                }
                if (fileContent != null && fileContent.SerializedContent != null)
                {
                    string content;
                    // Check if the SerializedContent is base64 or XML by detecting XML declaration
                    if (fileContent.SerializedContent.StartsWith("<?xml"))
                    {
                        // Directly deserialize XML content
                        content = Utils.DeserializeObject<string>(fileContent.SerializedContent);
                    }
                    else
                    {
                        // Decode base64 content
                        string decodedContent = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(fileContent.SerializedContent));
                        content = Utils.DeserializeObject<string>(decodedContent);
                    }

                    string filePath = Path.Combine(s_clientDirectory, fileContent.FileName ?? "Unnamed_file");
                    bool status = Utils.WriteToFileFromBinary(filePath, content);
                    if (!status)
                    {
                        throw new Exception("[Updater] Failed to write file");
                    }
                }
            }

            // Using the deserialized differences list to retrieve UniqueClientFiles
            List<string> filenameList = differencesList
                .Where(difference => difference != null && difference.Key == "-1")
                .SelectMany(difference => difference.Value?.Select(fileDetail => fileDetail.FileName) ?? new List<string>())
                .Distinct()
                .ToList();

            UpdateUILogs("Recieved request for files from Server");

            // Create list of FileContent to send back
            List<FileContent> fileContentToSend = new List<FileContent>();

            foreach (string filename in filenameList)
            {
                if (filename == differenceFileName)
                {
                    continue;
                }
                if (filename != null)
                {
                    string filePath = Path.Combine(s_clientDirectory, filename);
                    string? content = Utils.ReadBinaryFile(filePath) ?? throw new Exception("Failed to read file");
                    string? serializedContent = Utils.SerializeObject(content) ?? throw new Exception("Failed to serialize content");
                    FileContent fileContent = new FileContent(filename, serializedContent);
                    fileContentToSend.Add(fileContent);
                }
            }

            // Create DataPacket to send
            DataPacket dataPacketToSend = new DataPacket(DataPacket.PacketType.ClientFiles, fileContentToSend);

            // Serialize and send DataPacket
            string? serializedDataPacket = Utils.SerializeObject(dataPacketToSend) ?? throw new Exception("Failed to serialize DataPacket");
            UpdateUILogs("Sending requested files to server");
            Trace.WriteLine("Sending files to server");
            communicator.Send(serializedDataPacket, "FileTransferHandler", null);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[Updater] Error in DifferencesHandler: {ex.Message}");
        }
    }

    public void OnDataReceived(string serializedData)
    {
        try
        {

            Trace.WriteLine($"[Updater] FileTransferHandler received data");
            UpdateUILogs($"FileTransferHandler received data");
            PacketDemultiplexer(serializedData, _communicator);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[Updater] Error in OnDataReceived: {ex.Message}");
        }
    }

    public static event Action<string>? OnLogUpdate;

    public static void UpdateUILogs(string data)
    {
        OnLogUpdate?.Invoke(data);
    }
}
