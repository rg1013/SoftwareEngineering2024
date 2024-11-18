﻿using GrpcServer;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Networking.Communication;
using System.Net.Sockets;
using Networking;
using System.Diagnostics;
using System.Net;
using Grpc.Net.Client;
using System.Net.Http;
using GrpcClient;

namespace Networking.GrpcServices;
public class ServerServices : Server.ServerBase, ICommunicator
{
    private readonly object _maplock = new object();

    // Dictionary to store the client id and its ip and port
    private Dictionary<string, string>
        _clientIdToIpAndPort = new Dictionary<string, string>();

    // Dictionary to store the module name and its notification handler
    private Dictionary<string, INotificationHandler>
        _moduleToNotificationHanderMap = new();

    /// <summary>
    /// Adds a client to the server
    /// </summary>
    /// <param name="clientId">Client id of the client</param>
    /// <param name="socket">Socket of the client</param>
    /// <param name="ip">IP address of the client</param>
    /// <param name="port">Port of the client</param>
    public void AddClient(string clientId, TcpClient socket, string ip, string port)
    {
        Trace.WriteLine("[Networking] ServerServices.AddClient() function called");
        try
        {
            _clientIdToIpAndPort[clientId] = "http://" + ip + ":" + port;
            Trace.WriteLine("[Networking] Client added with clientID: " + clientId);
        }
        catch (Exception ex)
        {
            Trace.WriteLine("[Networking] Error in ServerServices.AddClient: " + ex.Message);
        }
    }

    /// <summary>
    /// client calls this function to connect to the server
    /// also notifies the modules that a new client has joined
    /// </summary>
    /// <param name="request">Request containing the ip and port of the server</param>
    /// <param name="context">Context of the grpc call</param>
    /// <returns>Response containing the connection success</returns>
    public override Task<connectResponse> connect(connectRequest request, ServerCallContext context)
    {
        string ip = request.Ip;
        string port = request.Port;

        try
        {
            foreach (KeyValuePair<string, INotificationHandler> moduleToNotificationHandler in
                                    _moduleToNotificationHanderMap)
            {
                TcpClient clientSocket = new();
                string module =
                    moduleToNotificationHandler.Key;
                INotificationHandler notificationHandler =
                    moduleToNotificationHandler.Value;
                notificationHandler.OnClientJoined(
                    clientSocket, ip, port);
                Trace.WriteLine("[Networking] Notifed " +
                    "module: " + module + " that new client" +
                    " has joined.");
            }

            return Task.FromResult(new connectResponse
            {
                ConnectionSuccess = true
            });
        }
        catch (Exception e)
        {
            Trace.WriteLine("[Networking] Error occured in serverServices.Connect() function");
            return Task.FromResult(new connectResponse
            {
                ConnectionSuccess = false
            });
        }
    }

    /// <summary>
    /// Removes a client from the dictionary
    /// </summary>
    /// <param name="clientId">Client id of the client to be removed</param>
    public void RemoveClient(string clientId)
    {
        Trace.WriteLine("[Networking] ServerServices.RemoveClient() function called");
        try
        {
            _clientIdToIpAndPort.Remove(clientId);
            Trace.WriteLine("[Networking] client removed with clientID: " + clientId);
        }
        catch (Exception e)
        {
            Trace.WriteLine("[Networking] Error in ServerServices.RemoveClient(): " + e.Message);
        }
    }

    /// <summary>
    /// Sends data to the mentioned destination or to all the clients
    /// </summary>
    /// <param name="serializedData">Serialized data to be sent</param>
    /// <param name="moduleName">Name of the module to which the data is to be sent</param>
    /// <param name="destination">Destination of the data</param>
    public void Send(string serializedData, string moduleName, string? destination)
    {
        Trace.WriteLine("[Networking] ServerServices.send() function called. ");

        // check if destination is not null then it must be id of a client,
        // then check if the client id is present in our map or not, if not
        // then print trace message and return 
        if (destination != null)
        {
            if (!_clientIdToIpAndPort.ContainsKey(destination))
            {
                Trace.WriteLine("[Networking] sending Failed. Client with ID: "
                    + destination + "does not exist in the room!");
                return;
            }
            SendDataToClient(serializedData, moduleName, destination);
            Trace.WriteLine("[Networking] data is sent to client with clientID: " + destination);
        }
        else // broadcast the message to all the clients
        {
            foreach (string clientId in _clientIdToIpAndPort.Keys)
            {
                SendDataToClient(serializedData, clientId, moduleName);
            }
        }
    }

    /// <summary>
    /// Sends data to the client with the given client id
    /// </summary>
    /// <param name="serializedData">Serialized data to be sent</param>
    /// <param name="destination">Destination of the data</param>
    /// <param name="moduleName">Name of the module to which the data is to be sent</param>
    private void SendDataToClient(string serializedData, string destination, string moduleName)
    {
        string clientAddress = _clientIdToIpAndPort[destination];
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        var channel = GrpcChannel.ForAddress(clientAddress, new GrpcChannelOptions
        {
            HttpHandler = handler
        });
        var client = new Client.ClientClient(channel);
        var request = new request
        {
            SerializedData = serializedData,
            Destination = destination,
            ModuleName = moduleName
        };
        client.receive(request);
    }

    /// <summary>
    /// Return the ip and port of the server
    /// </summary>
    /// <param name="serverIP">IP address of the server</param>
    /// <param name="serverPort">Port of the server</param>
    /// <returns>Returns the ip and port of the server</returns>
    public string Start(string? serverIP = null, string? serverPort = null)
    {
        IPAddress ip = IPAddress.Parse(FindIpAddress());
        int port = 7000;
        return (ip + ":" + port);
    }

    /// <summary>
    /// Finds IP4 address of the current machine which does not 
    /// ends with 1
    /// </summary>
    /// <returns>
    /// IP address of the current machine as a string
    /// </returns>
    private static string FindIpAddress()
    {
        Trace.WriteLine("[Networking] " +
            "CommunicatorServer.FindIpAddress() function called.");
        try
        {
            // get the IP address of the machine
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            // iterate through the ip addresses and return the
            // address if it is IPv4 and does not end with 1
            foreach (IPAddress ipAddress in host.AddressList)
            {
                // check if the address is IPv4 address
                if (ipAddress.AddressFamily ==
                    AddressFamily.InterNetwork)
                {
                    string address = ipAddress.ToString();
                    // return the IP address if it does not end
                    // with 1, as the loopback address ends with 1
                    if (address.Split(".")[3] != "1")
                    {
                        return ipAddress.ToString();
                    }
                }
            }
        }
        catch (Exception e)
        {
            Trace.WriteLine("[Networking] Error in " +
                "CommunicatorServer.FindIpAddress(): " +
                e.Message);
            return "null";
        }
        throw new Exception("[Networking] Error in " +
            "CommunicatorServer.FindIpAddress(): IPv4 address " +
            "not found on this machine!");
    }

    /// <summary>
    /// Finds a free TCP port on the current machine for the given
    /// IP address.
    /// </summary>
    /// <param name="ipAddress">
    /// IP address for which to find the free port.
    /// </param>
    /// <returns> The port number </returns>
    private static int FindFreePort(IPAddress ipAddress)
    {
        Trace.WriteLine("[Networking] " +
            "CommunicatorServer.FindFreePort() function called.");
        try
        {
            // start a tcp listener on port = 0, the tcp listener
            // will be assigned a port number
            TcpListener tcpListener = new(ipAddress, 0);
            tcpListener.Start();

            // return the port number of the tcp listener
            int port =
                ((IPEndPoint)tcpListener.LocalEndpoint).Port;
            tcpListener.Stop();
            return port;
        }
        catch (Exception e)
        {
            Trace.WriteLine("[Networking] Error in " +
                "CommunicatorServer.FindFreePort(): " +
                e.Message);
            return -1;
        }
    }

    public void Stop()
    {
        return;
    }

    /// <summary>
    /// Subscribes a module to the server
    /// </summary>
    /// <param name="moduleName">Name of the module</param>
    /// <param name="notificationHandler">Notification handler of the module</param>
    /// <param name="isHighPriority">True if the module is high priority, else False</param>
    public void Subscribe(string moduleName, INotificationHandler
        notificationHandler, bool isHighPriority)
    {
        Trace.WriteLine("[Networking] " +
            "CommunicatorServer.Subscribe() function called.");
        try
        {
            // store the notification handler of the module in our
            // map
            lock (_maplock)
            {
                // Store the notification handler of the module in our map
                _moduleToNotificationHanderMap[moduleName] = notificationHandler;
            }

            Trace.WriteLine("[Networking] Module: " + moduleName +
                " subscribed with priority [True for high/False" +
                " for low]: " + isHighPriority.ToString());
        }
        catch (Exception e)
        {
            Trace.WriteLine("[Networking] Error in " +
                "CommunicatorServer.Subscribe(): " + e.Message);
        }
    }


    /// <summary>
    /// Called remotely by the grpc client
    /// This function is called when a client sends data to the server
    /// The data is received by the server and passed to the appropriate module
    /// The module then calls the method "OnDataReceived" in the INotificationHandler
    /// </summary>
    /// <param name="request">Request containing the module name and serialized data</param>
    /// <param name="context">Context of the grpc call</param>
    /// <returns>Response containing the module name and serialized data</returns>
    public override Task<sendResponse> serverReceive(sendRequest request, ServerCallContext context)
    {
        Console.WriteLine("ServerReceive() function called");
        string moduleName = request.ModuleName;
        string serializedData = request.SerializedData;

        bool isModuleRegistered = false;

        // Find if module is registered
        lock (_maplock)
        {
            isModuleRegistered = _moduleToNotificationHanderMap.ContainsKey(moduleName);
        }

        // There is nothing to do if module is not registered
        if (!isModuleRegistered)
        {
            Trace.WriteLine($"[Networking] module {moduleName} does not have a handler.\n");
            return Task.FromResult(new sendResponse
            {

            });
        }

        _moduleToNotificationHanderMap[moduleName].OnDataReceived(serializedData);
        return Task.FromResult(new sendResponse
        {

        });
    }

    /// <summary>
    /// client calls this function to disconnect from the server
    /// also notifies the modules that the client has left
    /// </summary>
    /// <param name="request">Request containing the client id</param>
    /// <param name="context">Context of the grpc call</param>
    /// <returns>Response containing the client id</returns>
    public override Task<disconnectResponse> disconnect(disconnectRequest request, ServerCallContext context)
    {

        string clientId = request.ClientId;
        Trace.WriteLine("[Networking] Client: " + clientId +
                " has left. Removing client...");

        foreach (KeyValuePair<string, INotificationHandler> moduleToNotificationHandler in
                _moduleToNotificationHanderMap)
        {
            string moduleName =
                moduleToNotificationHandler.Key;
            INotificationHandler notificationHandler =
                moduleToNotificationHandler.Value;
            notificationHandler.OnClientLeft(clientId);
            Trace.WriteLine("[Networking] Notifed " +
                    "module: " + moduleName + " that the " +
                    "client: " + clientId + " has left.");
        }

        return Task.FromResult(new disconnectResponse
        {

        });
    }

}
