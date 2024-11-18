/******************************************************************************
 * Filename    = Server.cs
 *
 * Author      = Sai Hemanth Reddy
 * 
 * Project     = FileCloner
 *
 * Description = Manages server-side communication for the FileCloner application,
 *               handling client connections, message broadcasting, and targeted 
 *               communication between clients. The Server class maintains a list 
 *               of connected clients and ensures messages are delivered to the 
 *               appropriate recipients.
 *****************************************************************************/

using Networking.Communication;
using System.Net.Sockets;
using System.Net;
using Networking;
using Networking.Serialization;

namespace FileCloner.Models.NetworkService
{
    /// <summary>
    /// The Server class manages incoming and outgoing communication with clients,
    /// handling data routing, client management, and logging for the FileCloner application.
    /// </summary>
    public class Server : INotificationHandler
    {
        // Instance of the server communicator for managing connections
        private static CommunicatorServer server =
            (CommunicatorServer)CommunicationFactory.GetCommunicator(isClientSide: false);

        // Counter for assigning unique IDs to clients as they join
        private static int clientid = 0;

        // Dictionary to store the mapping of client IP addresses to their unique IDs
        private static Dictionary<string, string> clientList = new();

        // Serializer for handling message serialization and deserialization
        private static ISerializer serializer = new Serializer();

        // Delegate for logging actions, e.g., writing to UI or console
        private readonly Action<string> logAction;

        public static Server _ServerInstance;

        public static Server GetServerInstance()
        {
            return _ServerInstance;
        }

        public void SetUser(string clientId, TcpClient socket)
        {
            string clientIpAddress = ((IPEndPoint)socket.Client.RemoteEndPoint).Address.ToString();
            clientList.Add(clientIpAddress, clientId);

        }

        /// <summary>
        /// Initializes the server, starts listening on the specified port,
        /// and subscribes to the message handler for the module.
        /// </summary>
        /// <param name="logAction">Delegate for logging status updates and errors.</param>
        public Server(Action<string> logAction)
        {
            this.logAction = logAction;
            _ServerInstance = this;
            try
            {
                // Start server on the specified port and subscribe for notifications
                server.Start(serverPort: "8080");
                server.Subscribe(Constants.moduleName, this, false);
                logAction.Invoke("[Server] Started successfully");
            }
            catch (Exception e)
            {
                throw new Exception("[Server] Not started: " + e.Message);
            }
        }

        /// <summary>
        /// Handles data received from clients, determines if it's a broadcast or 
        /// directed message, and routes it accordingly.
        /// </summary>
        /// <param name="serializedData">The serialized data received from a client.</param>
        public void OnDataReceived(string serializedData)
        {
            try
            {
                // Deserialize the message to process its details
                Message message = serializer.Deserialize<Message>(serializedData);

                if (message == null)
                {
                    return;
                }

                // Check if the message is a broadcast
                if (message.To == Constants.broadcast)
                {
                    // Send to all connected clients if it's a broadcast
                    server.Send(serializedData, Constants.moduleName, null);
                }
                else
                {
                    // Targeted message; find and send to the specified client
                    string targetClientId = clientList[message.To];
                    server.Send(serializedData, Constants.moduleName, targetClientId);
                }
            }
            catch (Exception e)
            {
                logAction.Invoke("[Server] Error in sending data: " + e.Message);
            }
        }

        /// <summary>
        /// Removes a client from the client list when they disconnect.
        /// </summary>
        /// <param name="clientId">The unique ID of the client that left.</param>
        public void OnClientLeft(string clientId)
        {
            // Find the client in the dictionary by clientId
            var clientEntry = clientList.FirstOrDefault(entry => entry.Value == clientId);
            if (!string.IsNullOrEmpty(clientEntry.Key))
            {
                logAction.Invoke($"[Server] {clientList[clientEntry.Key]} Left");
                clientList.Remove(clientEntry.Key);
            }
        }

        /// <summary>
        /// Adds a client to the server's client list when they connect.
        /// </summary>
        /// <param name="client">The TcpClient instance representing the connected client.</param>
        public void OnClientJoined(TcpClient client)
        {
            // Retrieve the client's IP address
            string clientIpAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();

            // Generate a unique ID for the client based on the clientid counter
            string clientUniqueId = clientid.ToString();
            logAction.Invoke($"[Server] {clientIpAddress} Joined");

            // Add the client to the client list and increment the counter
          //  server.AddClient(clientUniqueId, client);
          //  clientList.Add(clientIpAddress, clientUniqueId);
          //  clientid++;
        }

        /// <summary>
        /// Stops the server and terminates all client connections.
        /// </summary>
        public void Stop()
        {
            server.Stop();
        }
    }
}
