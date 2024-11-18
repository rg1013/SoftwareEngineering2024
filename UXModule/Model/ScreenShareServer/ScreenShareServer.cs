﻿using Networking.Communication;
using Networking;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Timers;
using System.Net.Sockets;
using Networking.Queues;
using System.Net;
using UXModule.ViewModel;

namespace UXModule.Model
{
    
    /// Represents the data model for screen sharing on the server side machine.
    
    public class ScreenshareServer :
        INotificationHandler, // To receive packets from the networking.
        ITimerManager,        // Handles the timeout for screen sharing of clients.
        IDisposable           // Handle cleanup work for the allocated resources.
    {
        
        /// The only singleton instance for this class.
        
        private static ScreenshareServer? _instance;

        
        /// The networking object used to subscribe to the networking module
        /// and to send the packets to the clients.
        
        public  ICommunicator? _communicator;

        
        /// The subscriber which should be notified when subscribers list change.
        /// Here it will be the view model.
        
        private readonly IMessageListener _listener;

        
        /// The map between each client ID and their corresponding "SharedScreenObject"
        /// to keep track of all the active subscribers (screen sharers).
        
        private readonly Dictionary<string, SharedClientScreen> _subscribers;

        
        /// Track whether Dispose has been called.
        
        private bool _disposed;

        
        /// Creates an instance of "ScreenshareServer" which represents the
        /// data model for screen sharing on the server side machine.
        
     
        protected ScreenshareServer(IMessageListener listener, bool isDebugging)
        {
            if (!isDebugging)
            {
                // Get an instance of a communicator object.
                _communicator = CommunicationFactory.GetCommunicator(false);
                _communicator.Subscribe(Utils.ModuleIdentifier, this, true);
                _communicator.Start();
            }

            // Initialize the rest of the fields.
            _subscribers = new();
            _listener = listener;
            _disposed = false;

            Trace.WriteLine(Utils.GetDebugMessage("Successfully created an instance of ScreenshareServer", withTimeStamp: true));
        }

        
        /// Destructor for the class that will perform some cleanup tasks.
        /// This destructor will run only if the Dispose method does not get called.
        /// It gives the class the opportunity to finalize.
        
        ~ScreenshareServer()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(disposing: false) is optimal in terms of
            // readability and maintainability.
            Dispose(disposing: false);
        }

        
        /// Implements "INotificationHandler". It will be invoked when a data packet
        /// comes for the screen share module from the client to the server. Based on
        /// the header in the packet received, it will do further processing.
        
        /// <param name="serializedData">
        /// Data received inside the packet from the client.
        /// </param>
        public void OnDataReceived(string serializedData)
        {
            try
            {
                DataPacket? packet = JsonSerializer.Deserialize<DataPacket>(serializedData);

                if (packet == null)
                {
                    Trace.WriteLine(Utils.GetDebugMessage($"Not able to deserialize data packet: {serializedData}", withTimeStamp: true));
                    return;
                }

                // Extract different fields from the object of the "DataPacket".
                string clientId = packet.Id;
                string clientName = packet.Name;
                ClientDataHeader header = Enum.Parse<ClientDataHeader>(packet.Header);
                string clientData = packet.Data;

                Trace.WriteLine(header);
                // Based on the packet header, do further processing.
                switch (header)
                {
                    case ClientDataHeader.Register:
                        RegisterClient(clientId, clientName);
                        break;
                    case ClientDataHeader.Deregister:
                        DeregisterClient(clientId);
                        break;
                    case ClientDataHeader.Image:
                        PutImage(clientId, clientData, packet.ChangedPixels, packet.IsFull);
                        break;
                    case ClientDataHeader.Confirmation:
                        UpdateTimer(clientId);
                        break;
                    default:
                        throw new Exception($"Unknown header {packet.Header}");
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(Utils.GetDebugMessage($"Exception while processing the packet: {e.Message}", withTimeStamp: true));
            }
        }


        /// Implements "INotificationHandler". Not required by the screen share server module.

        protected string GetAddressFromSocket(TcpClient socket, bool otherEnd = false)
        {
            IPEndPoint endPoint = null;
            if (!otherEnd)
            {
                endPoint = (IPEndPoint?)socket.Client.LocalEndPoint;
            }
            else
            {
                endPoint = (IPEndPoint?)socket.Client.RemoteEndPoint;
            }
            if (endPoint == null)
            {
                return "";
            }
            string ipAddress = endPoint.Address.MapToIPv4().ToString();
            string port = endPoint.Port.ToString();

            // using underscores since apparently fileNames cannot have ':'
            string address = GetConcatenatedAddress(ipAddress, port);
            return address;
        }

        protected string GetConcatenatedAddress(string ipAddress, string port)
        {
            return $"{ipAddress}_{port}";
        }


#pragma warning disable CA1822 // Mark members as static.
        public void OnClientJoined(TcpClient socket) {
           /* Trace.WriteLine("aeaeaeaeaeaeae");
            string address = GetAddressFromSocket(socket, otherEnd: true);
            Trace.WriteLine(address);
            // _logger.Log($"Client Joined : {address}");
            Console.WriteLine($"Client Joined : {address}");
            /*  lock (_syncLock)
              {
                  _clientDictionary.Add(address, socket);
              }*/
          /*  string serverIP = address.Split('_')[0];
            string serverPort = address.Split('_')[1];
            _communicator.AddClient(serverIP, socket); 
           */ 
            
          //  _fileReceiverServer.AddClient($"{serverIP}:{serverPort}", socket);

        }
#pragma warning restore CA1822 // Mark members as static.


        /// Implements "INotificationHandler". It is invoked by the Networking Communicator
        /// when a client leaves the meeting.

        public void OnClientLeft(string clientId)
        {
            Debug.Assert(_subscribers != null, Utils.GetDebugMessage("_subscribers is found null"));

            // Deregister the client if it was sharing screen.
            if (_subscribers.ContainsKey(clientId))
            {
                DeregisterClient(clientId);
            }
        }

        
        /// Implements "ITimerManager". Callback which will be invoked when the timeout occurs for the
        /// CONFIRMATION packet not received by the client.
        
  
        public void OnTimeOut(object? source, ElapsedEventArgs e, string clientId)
        {
            DeregisterClient(clientId);
            Trace.WriteLine(Utils.GetDebugMessage($"Timeout occurred for the client with id: {clientId}", withTimeStamp: true));
        }

        
        /// Implement "IDisposable". Disposes the managed and unmanaged resources.
        
        public void Dispose()
        {
            Dispose(disposing: true);

            // This object will be cleaned up by the Dispose method.
            // Therefore, we should call GC.SuppressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        
        /// Gets a singleton instance of "ScreenshareServer" class.
        
       
        public static ScreenshareServer GetInstance(IMessageListener listener, bool isDebugging = false)
        {
            Debug.Assert(listener != null, Utils.GetDebugMessage("listener is found null"));

            // Create a new instance if it was null before.
            _instance ??= new(listener, isDebugging);
            return _instance;
        }

        
        /// Used to send various data packets to the clients.
        /// Also provide them the resolution of the image to send if asking
        /// the clients to send the image packet.
        
     
        public void BroadcastClients(List<string> clientIds, string headerVal, (int Rows, int Cols) numRowsColumns)
        {
            Debug.Assert(_communicator != null, Utils.GetDebugMessage("_communicator is found null"));
            Debug.Assert(clientIds != null, Utils.GetDebugMessage("list of client Ids is found null"));

            // If there are no clients to broadcast to.
            Trace.WriteLine("cccccccccccccccccccccccccc");
            if (clientIds.Count == 0) return;

            // Validate header value.
            try
            {
                ServerDataHeader _ = Enum.Parse<ServerDataHeader>(headerVal);
            }
            catch (Exception e)
            {
                Trace.WriteLine(Utils.GetDebugMessage($"Failed to parse the header {headerVal} : {e.Message}", withTimeStamp: true));
                return;
            }

            // Serialize the data to send.
            try
            {

                Trace.WriteLine("fffffff");

                int product = numRowsColumns.Rows * numRowsColumns.Cols;
                string serializedData = JsonSerializer.Serialize(product);

                // Create the data packet to send.
                DataPacket packet = new("2", "Server", headerVal, serializedData, false, false, null);

                // Serialize the data packet to send to clients.
                string serializedPacket = JsonSerializer.Serialize(packet);

                // Send data packet to all the clients mentioned.
                foreach (string clientId in clientIds)
                {
                    Trace.WriteLine("fggggg");
                    _communicator.Send(serializedPacket, Utils.ModuleIdentifier, clientId);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(Utils.GetDebugMessage($"Exception while sending the packet to the client: {e.Message}", withTimeStamp: true));
            }
        }

        
        /// It executes in two distinct scenarios.
        /// If disposing equals true, the method has been called directly
        /// or indirectly by a user's code. Managed and unmanaged resources
        /// can be disposed.
        /// If disposing equals false, the method has been called by the
        /// runtime from inside the destructor and we should not reference
        /// other objects. Only unmanaged resources can be disposed.
        
      
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (_disposed) return;

            // If disposing equals true, dispose all managed
            // and unmanaged resources.
            if (disposing)
            {
                List<SharedClientScreen> sharedClientScreens;

                // Acquire lock because timer threads could also execute simultaneously.
                lock (_subscribers)
                {
                    sharedClientScreens = _subscribers.Values.ToList();
                    _subscribers.Clear();
                }

                // Deregister all the clients.
                foreach (SharedClientScreen client in sharedClientScreens)
                {
                    DeregisterClient(client.Id);
                }

                _instance = null;
            }

            // Call the appropriate methods to clean up unmanaged resources here.

            // Now disposing has been done.
            _disposed = true;
        }

        
        /// Add this client to list of screen sharers. It also notifies the view
        /// model that a new client has started presenting screen.
        
  
        private void RegisterClient(string clientId, string clientName)
        {
            Debug.Assert(_subscribers != null, Utils.GetDebugMessage("_subscribers is found null"));
            
            // Acquire lock because timer threads could also execute simultaneously.
           
            lock (_subscribers)
            {
                // Check if the clientId is present in the screen sharers list.
                if (_subscribers.ContainsKey(clientId))
                {
                    Trace.WriteLine(Utils.GetDebugMessage($"Trying to register an already registered client with id {clientId}", withTimeStamp: true));
                    return;
                }

                try
                {
                    // Add this client to the list of screen sharers.
                    _subscribers.Add(clientId, new(clientId, clientName, this));
                }
                catch (Exception e)
                {
                    Trace.WriteLine(Utils.GetDebugMessage($"Error adding client to the list of screen sharers: {e.Message}", withTimeStamp: true));
                    return;
                }
            }

            NotifyUX();
            NotifyUX(clientId, clientName, hasStarted: true);

            Trace.WriteLine(Utils.GetDebugMessage($"Successfully registered the client- Id: {clientId}, Name: {clientName}", withTimeStamp: true));
        }

        
        /// Remove this client from the list of screen sharers. It also
        /// asks the client object to stop all its processing and notify the
        /// view model that a client has stopped screen sharing.
  
        private void DeregisterClient(string clientId)
        {
            Debug.Assert(_subscribers != null, Utils.GetDebugMessage("_subscribers is found null"));

            SharedClientScreen client;

            // Acquire lock because timer threads could also execute simultaneously.
            lock (_subscribers)
            {
                // Check if the clientId is present in the screen sharers list.
                if (!_subscribers.ContainsKey(clientId))
                {
                    Trace.WriteLine(Utils.GetDebugMessage($"Trying to deregister a client with id {clientId} which is not present in subscribers list", withTimeStamp: true));
                    return;
                }

                // Remove the client from the list of screen sharers.
                client = _subscribers[clientId];
                _ = _subscribers.Remove(clientId);
            }

            NotifyUX();
            NotifyUX(clientId, client.Name, hasStarted: false);

            // Stop all processing for this client.
            try
            {
                client.StopProcessing();
            }
            catch (OperationCanceledException e)
            {
                Trace.WriteLine(Utils.GetDebugMessage($"Task canceled for the client with id {clientId}: {e.Message}", withTimeStamp: true));
            }
            catch (Exception e)
            {
                Trace.WriteLine(Utils.GetDebugMessage($"Failed to stop the task for the removed client with id {clientId}: {e.Message}", withTimeStamp: true));
            }
            finally
            {
                client.Dispose();
            }

            Trace.WriteLine(Utils.GetDebugMessage($"Successfully removed the client with Id {clientId}", withTimeStamp: true));
        }

        
        /// Adds the image received from the client to the client's image queue.
        
 
        private void PutImage(string clientId, string image, List<PixelDifference> change, bool full)
        {
            Trace.WriteLine("aaaaaaaaaaaaaaaaaaaaa");
                             
            Debug.Assert(_subscribers != null, Utils.GetDebugMessage("_subscribers is found null"));

            // Acquire lock because timer threads could also execute simultaneously.
            lock (_subscribers)
            {
                // Check if the clientId is present in the screen sharers list.
                if (!_subscribers.ContainsKey(clientId))
                {
                    Trace.WriteLine(Utils.GetDebugMessage($"Client with id {clientId} is not present in subscribers list", withTimeStamp: true));
                    return;
                }

                // Put the image to the client's image queue.
                try
                {
                    SharedClientScreen client = _subscribers[clientId];
                    client.PutImage(image, client.TaskId, change);
                }
                catch (Exception e)
                {
                    Trace.WriteLine(Utils.GetDebugMessage($"Exception while processing the received image: {e.Message}", withTimeStamp: true));
                }
            }

            Trace.WriteLine(Utils.GetDebugMessage($"Successfully received image of the client with Id: {clientId}", withTimeStamp: true));
        }

        
        /// Reset the timer for the client.
        

        private void UpdateTimer(string clientId)
        {
            Debug.Assert(_subscribers != null, Utils.GetDebugMessage("_subscribers is found null"));

            // Acquire lock because timer threads could also execute simultaneously.
            lock (_subscribers)
            {
                // Check if the clientId is present in the screen sharers list.
                if (!_subscribers.ContainsKey(clientId))
                {
                    Trace.WriteLine(Utils.GetDebugMessage($"Client with id {clientId} is not present in subscribers list", withTimeStamp: true));
                    return;
                }

                // Reset the timer for the client.
                try
                {
                    SharedClientScreen client = _subscribers[clientId];
                    client.UpdateTimer();

                    // Send Confirmation packet back to the client.
                    BroadcastClients(new() { clientId }, nameof(ServerDataHeader.Confirmation), (0, 0));
                }
                catch (Exception e)
                {
                    Trace.WriteLine(Utils.GetDebugMessage($"Failed to update the timer for the client with id {clientId}: {e.Message}", withTimeStamp: true));
                }
            }
        }

        
        /// Notifies the view model with the updates list of screen sharers.
        
        private void NotifyUX()
        {
            Debug.Assert(_subscribers != null, Utils.GetDebugMessage("_subscribers is found null"));
            Debug.Assert(_listener != null, Utils.GetDebugMessage("_listener is found null"));

            List<SharedClientScreen> sharedClientScreens;

            // Acquire lock because timer threads could also execute simultaneously.
            lock (_subscribers)
            {
                sharedClientScreens = _subscribers.Values.ToList();
            }

            _listener.OnSubscribersChanged(sharedClientScreens);
        }

        
        /// Notifies the view model about a client has either started or stopped screen sharing.
        
        /// <param name="clientId">
        /// Id of the client who started or stopped screen sharing.
        /// </param>
        /// <param name="clientName">
        /// Name of the client who started or stopped screen sharing.
        /// </param>
        /// <param name="hasStarted">
        /// Whether the client has started or stopped screen sharing.
        /// </param>
        private void NotifyUX(string clientId, string clientName, bool hasStarted)
        {
            if (hasStarted)
            {
                _listener.OnScreenshareStart(clientId, clientName);
            }
            else
            {
                _listener.OnScreenshareStop(clientId, clientName);
            }
        }
    }
}
