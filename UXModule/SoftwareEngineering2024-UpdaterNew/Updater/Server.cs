/******************************************************************************
* Filename    = Server.cs
*
* Author      = Amithabh A and Garima Ranjan
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = Server side sending and receiving files logic
*****************************************************************************/

using Networking.Communication;
using Networking;
using System.Net.Sockets;
using System.Diagnostics;

namespace Updater;

public class Server : INotificationHandler
{
    static int s_clientCounter = 0; // Counter for unique client IDs
    private static readonly string s_serverDirectory = AppConstants.ToolsDirectory;

    private readonly BinarySemaphore _semaphore = new();

    private ICommunicator? _communicator;

    public static event Action<string>? NotificationReceived; // Event to notify the view model
    public string _clientID = "";
    private readonly Dictionary<string, TcpClient> _clientConnections = []; // Track clients

    /// <summary>
    /// Start the server
    /// </summary>
    /// <param name="ip">IP address of server</param>
    /// <param name="port">port number of server</param>
    public void Start(string ip, string port)
    {
        try
        {
            _communicator = CommunicationFactory.GetCommunicator(false);

            // Starting the server
            string result = _communicator.Start(ip, port);
            UpdateUILogs($"Server started on {result}");
            UpdateUILogs($"Monitoring {s_serverDirectory}");

            // Subscribing the "FileTransferHandler" for handling notifications
            _communicator.Subscribe("FileTransferHandler", this);
        }
        catch (Exception ex)
        {
            UpdateUILogs($"Error in server start: {ex.Message}");
        }
    }

    /// <summary>
    /// Stop the server
    /// </summary>
    public void Stop()
    {
        try
        {
            _communicator?.Stop();
            UpdateUILogs("Server stopped.");
        }
        catch (Exception ex)
        {
            UpdateUILogs($"Error stopping the server: {ex.Message}");
        }
    }

    /// <summary>
    /// Send SyncUp request to client
    /// </summary>
    /// <param name="clientId">ID of the client</param>
    private void SyncUp(string clientId)
    {
        try
        {
            UpdateUILogs($"Sending sync up request to client {clientId}");
            string serializedSyncUpPacket = Utils.SerializedSyncUpPacket();

            // Write equivalent of this: 
            // UpdateUILogs("Syncing Up with the server");
            Trace.WriteLine($"[Updater] Sending SyncUp request dataPacket to client: {clientId}");
            if (_communicator != null)
            {
                _communicator.Send(serializedSyncUpPacket, "FileTransferHandler", clientId);
            }
            else
            {
                UpdateUILogs("Communicator is null");
            }
        }
        catch (Exception ex)
        {
            UpdateUILogs($"Error in SyncUp: {ex.Message}");
        }

    }

    /// <summary>
    /// Implementation of Wait before SyncUp
    /// </summary>
    public void RequestSyncUp(string clientId)
    {
        try
        {
            _semaphore.Wait();
            _clientID = clientId;
            SyncUp(clientId);
        }
        catch (Exception ex)
        {
            UpdateUILogs($"Error in RequestSyncUp: {ex.Message}");
        }
    }


    /// <summary>
    /// Complete the sync by Signalling _semaphore
    /// </summary>
    public void CompleteSync()
    {
        try
        {
            _semaphore.Signal();
        }
        catch (Exception ex)
        {
            UpdateUILogs($"Error in CompleteSync: {ex.Message}");
        }
    }

    public static void UpdateUILogs(string message)
    {
        NotificationReceived?.Invoke(message);
    }




    /// <summary>
    /// Demultiplex the data packet
    /// </summary>
    /// <param name="serializedData">Serialized data packet</param>
    /// <param name="communicator">Communicator object</param>
    /// <param name="server">Server object</param>
    /// <param name="clientId">Client ID</param>
    public static void PacketDemultiplexer(string serializedData, ICommunicator communicator, Server server, string clientId)
    {
        try
        {
            // Deserialize data
            DataPacket dataPacket = Utils.DeserializeObject<DataPacket>(serializedData);

            // Check PacketType
            switch (dataPacket.DataPacketType)
            {
                case DataPacket.PacketType.SyncUp:
                    SyncUpHandler(dataPacket, communicator, server, clientId);
                    break;
                case DataPacket.PacketType.Metadata:
                    MetadataHandler(dataPacket, communicator, clientId);
                    break;
                case DataPacket.PacketType.ClientFiles:
                    ClientFilesHandler(dataPacket, communicator, server, clientId);
                    break;
                default:
                    throw new Exception("Invalid PacketType");
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Error in PacketDemultiplexer: {ex.Message}");
        }
    }

    /// <summary>
    /// Handler for SyncUp request from client
    /// </summary>
    /// <param name="dataPacket">Data packet</param>
    /// <param name="communicator">Communicator object</param>
    /// <param name="server">Server object</param>
    /// <param name="clientId">Client ID</param>
    private static void SyncUpHandler(DataPacket dataPacket, ICommunicator communicator, Server server, string clientId)
    {
        try
        {
            // Start new thread for client for communication
            Thread thread = new Thread(() => server.RequestSyncUp(clientId));
            thread.Start();
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[Updater] Error in SyncUpHandler: {ex.Message}");
        }
    }

    /// <summary>
    /// Metadata dataPacket Handler
    /// </summary>
    /// <param name="dataPacket">Data packet</param>
    /// <param name="communicator">Communicator object</param>
    /// <param name="clientId">Client ID</param>
    private static void MetadataHandler(DataPacket dataPacket, ICommunicator communicator, string clientId)
    {
        try
        {
            UpdateUILogs($"Received {clientId} metadata");
            // Extract metadata of client directory
            List<FileContent> fileContents = dataPacket.FileContentList;

            if (!fileContents.Any())
            {
                UpdateUILogs("No file content received in the data packet.");
                throw new Exception("No file content received in the data packet.");
            }

            // Process the first file content
            FileContent fileContent = fileContents[0];
            string? serializedContent = fileContent.SerializedContent;

            Trace.WriteLine("[Updater] " + serializedContent ?? "Serialized content is null");

            // Deserialize the client metadata
            List<FileMetadata>? metadataClient;
            if (serializedContent != null)
            {
                metadataClient = Utils.DeserializeObject<List<FileMetadata>>(serializedContent);
            }
            else
            {
                metadataClient = null;
            }
            if (metadataClient == null)
            {
                UpdateUILogs("Deserialized client metadata is null");
                throw new Exception("[Updater] Deserialized client metadata is null");
            }

            Trace.WriteLine("[Updater]: Metadata from client received");

            // Generate metadata of server
            List<FileMetadata>? metadataServer = new DirectoryMetadataGenerator(s_serverDirectory).GetMetadata();
            if (metadataServer == null)
            {
                UpdateUILogs("Metadata server is null");
                throw new Exception("Metadata server is null");
            }
            Trace.WriteLine("[Updater] Metadata from server generated");

            // Compare metadata and get differences
            DirectoryMetadataComparer comparerInstance = new DirectoryMetadataComparer(metadataServer, metadataClient);
            List<MetadataDifference> differences = comparerInstance.Differences;

            // Serialize and save differences to C:\temp\ folder
            string serializedDifferences = Utils.SerializeObject(differences);
            string tempFilePath = @$"{Server.s_serverDirectory}\differences.xml";

            if (string.IsNullOrEmpty(serializedDifferences))
            {
                Trace.WriteLine("[Updater] Serialization of differences failed or resulted in an empty string.");
                return; // Exit if serialization fails
            }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(tempFilePath)!);
                File.WriteAllText(tempFilePath, serializedDifferences);
                Server.UpdateUILogs($"Differences file saved to {tempFilePath}");
                Trace.WriteLine($"[Updater] Differences file saved to {tempFilePath}");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[Updater] Error saving differences file: {ex.Message}");
            }

            // Prepare data to send to client
            List<FileContent> fileContentsToSend = new List<FileContent>
                {
                    // Added difference file to be sent to client
                    new FileContent("differences.xml", serializedDifferences)
                };

            // Retrieve and add unique server files to fileContentsToSend
            foreach (string filename in comparerInstance.UniqueServerFiles)
            {
                string filePath = Path.Combine(Server.s_serverDirectory, filename);
                string? content = Utils.ReadBinaryFile(filePath);

                if (content == null)
                {
                    Trace.WriteLine($"Warning: Content of file {filename} is null, skipping.");
                    continue; // Skip to the next file instead of throwing an exception
                }

                Trace.WriteLine($"[Updater] Content length of {filename}: {content.Length}");

                // Serialize file content and create FileContent object
                string serializedFileContent = Utils.SerializeObject(content);
                if (string.IsNullOrEmpty(serializedFileContent))
                {
                    Trace.WriteLine($"[Updater] Warning: Serialized content for {filename} is null or empty.");
                    continue; // Skip to next file if serialization fails
                }

                FileContent fileContentToSend = new FileContent(filename, serializedFileContent);
                fileContentsToSend.Add(fileContentToSend);
            }

            // Create DataPacket after all FileContents are ready
            DataPacket dataPacketToSend = new DataPacket(DataPacket.PacketType.Differences, fileContentsToSend);
            Trace.WriteLine($"[Updater] Total files to send: {fileContentsToSend.Count}");

            // Serialize DataPacket
            string serializedDataPacket = Utils.SerializeObject(dataPacketToSend);

            try
            {
                Server.UpdateUILogs($"Sending files to client and waiting to recieve files from client {clientId}");
                communicator.Send(serializedDataPacket, "FileTransferHandler", clientId);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[Updater] Error sending data to client: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[Updater] Error sending data to client: {ex.Message}");
        }
    }

    /// <summary>
    /// ClientFiles dataPacket handler
    /// </summary>
    /// <param name="dataPacket">Data packet</param>
    /// <param name="communicator">Communicator object</param>
    /// <param name="server">Server object</param>
    /// <param name="clientId">Client ID</param>
    private static void ClientFilesHandler(DataPacket dataPacket, ICommunicator communicator, Server server, string clientId)
    {
        try
        {
            Server.UpdateUILogs("Recieved files from client");
            // File list
            List<FileContent> fileContentList = dataPacket.FileContentList;

            // Get files
            foreach (FileContent fileContent in fileContentList)
            {
                if (fileContent != null && fileContent.SerializedContent != null && fileContent.FileName != null)
                {
                    string content = Utils.DeserializeObject<string>(fileContent.SerializedContent);
                    string filePath = Path.Combine(Server.s_serverDirectory, fileContent.FileName);
                    bool status = Utils.WriteToFileFromBinary(filePath, content);

                    if (!status)
                    {
                        throw new Exception("Failed to write file");
                    }
                }
            }

            Server.UpdateUILogs("Successfully received client's files");
            Trace.WriteLine("[Updater] Successfully received client's files");

            // Broadcast client's new files to all clients
            dataPacket.DataPacketType = DataPacket.PacketType.Broadcast;

            // Serialize packet
            string serializedPacket = Utils.SerializeObject(dataPacket);

            Server.UpdateUILogs("Broadcasting the new files");
            Trace.WriteLine("[Updater] Broadcasting the new files");
            try
            {
                communicator.Send(serializedPacket, "FileTransferHandler", null); // Broadcast to all clients
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[Updater] Error sending data to client: {ex.Message}");
            }

            // Wait for one second
            System.Threading.Thread.Sleep(1000);
            server.CompleteSync();
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[Updater] Error in ClientFilesHandler: {ex.Message}");
        }
    }

    public void OnDataReceived(string serializedData)
    {
        try
        {
            Trace.WriteLine("[Updater] FileTransferHandler received data");
            DataPacket deserializedData = Utils.DeserializeObject<DataPacket>(serializedData);
            if (deserializedData == null)
            {
                Trace.WriteLine("Deserialized data is null.");
            }
            else
            {
                Trace.WriteLine("[Updater] Read received data Successfully");
                PacketDemultiplexer(serializedData, _communicator, this, _clientID);
            }

        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[Updater] Deserialization failed: {ex.Message}");
        }
        finally
        {
        }
    }

    public void OnClientJoined(TcpClient socket)
    {
        try
        {
            // Generate a unique client ID
            string clientId = $"Client{Interlocked.Increment(ref s_clientCounter)}"; // Use Interlocked for thread safety

            Trace.WriteLine($"[Updater] FileTransferHandler detected new client connection: {socket.Client.RemoteEndPoint}, assigned ID: {clientId}");
            Server.UpdateUILogs($"Detected new client connection: {socket.Client.RemoteEndPoint}, assigned ID: {clientId}");

            _clientConnections.Add(clientId, socket); // Add client connection to the dictionary
            _communicator?.AddClient(clientId, socket); // Use the unique client ID

            // Start new thread for client for communication
            Thread thread = new Thread(() => this.RequestSyncUp(clientId));
            thread.Start();
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[Updater] Error in OnClientJoined: {ex.Message}");
        }
    }

    public void OnClientLeft(string clientId)
    {
        try
        {
            if (_clientConnections.Remove(clientId))
            {
                Server.UpdateUILogs($"Detected client {clientId} disconnected");
                Trace.WriteLine($"[Updater] FileTransferHandler detected client {clientId} disconnected");
            }
            else
            {
                Trace.WriteLine($"[Updater] Client {clientId} was not found in the connections.");
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[Updater] Error in OnClientLeft: {ex.Message}");
        }
    }
}

