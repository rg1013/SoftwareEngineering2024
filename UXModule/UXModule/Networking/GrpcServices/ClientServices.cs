using Grpc.Core;
using Grpc.Net.Client;
using GrpcClient;
using Networking;
using Networking.Communication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Net.Http;
using GrpcServer;

namespace Networking.GrpcServices;
public class ClientServices : Client.ClientBase, ICommunicator
{
    private readonly object _maplock = new object();

    private string _clientId;
    private string _serverAddress;
    private readonly Dictionary<string, INotificationHandler>
        _moduleToNotificationHanderMap = new();

    /// <summary>
    /// Not required for the client services
    /// </summary>
    [ExcludeFromCodeCoverage]
    public void AddClient(string clientId, TcpClient socket, string ip = null, string port = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Not required for the client services
    /// </summary>
    [ExcludeFromCodeCoverage]
    public void RemoveClient(string clientId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Sends data to the server
    /// </summary>
    /// <param name="serializedData">Serialized data to be sent</param>
    /// <param name="moduleName">Name of the module to which the data is to be sent</param>
    /// <param name="destination">Destination of the data</param>
    public void Send(string serializedData, string moduleName, string? destination)
    {
        Trace.WriteLine("[Networking] clientServices.Send() " +
            "function called.");
        try
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            var channel = GrpcChannel.ForAddress(_serverAddress, new GrpcChannelOptions
            {
                HttpHandler = handler
            });
            var server = new Server.ServerClient(channel);
            var sendRequest = new sendRequest
            {
                SerializedData = serializedData,
                Destination = destination ?? string.Empty,
                ModuleName = moduleName
            };
            server.serverReceive(sendRequest);
            Trace.WriteLine("[Networking] serverReceive called " +
                "for data from module: " + moduleName);
        }
        catch (Exception e)
        {
            Trace.WriteLine("[Networking] Error in clientServies.Send() : " + e.Message);
        }

    }

    /// <summary>
    /// Connects to the grpc server
    /// </summary>
    /// <param name="serverIP">IP address of the server</param>
    /// <param name="serverPort">Port of the server</param>
    /// <returns>Returns "success" if the connection is successful, else returns "failure"</returns>
    public string Start(string serverIP, string serverPort)
    {
        Trace.WriteLine("[Networking] ClientServices.Start()" +
            " function called.");
        IPAddress ip = IPAddress.Parse(FindIpAddress());
        int port = 7009;
        try
        {
            _serverAddress = "http://" + serverIP + ":" + serverPort;
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            var channel = GrpcChannel.ForAddress(_serverAddress, new GrpcChannelOptions
            {
                HttpHandler = handler
            });

            //channel = GrpcChannel.ForAddress(serverAddress);
            var client = new Server.ServerClient(channel);
            var connectRequest = new connectRequest
            {
                Ip = ip.ToString(),
                Port = port.ToString() // default port that been set in the appsettings.json
            };
            connectResponse reply = client.connect(connectRequest);
            Trace.WriteLine("[Networking] ClientServices" +
               " started.");
            return reply.ConnectionSuccess ? "success" : "failure";
        }
        catch (Exception e)
        {
            Trace.WriteLine("[Networking] Error in " +
               "ClientServices.Start(): " + e.Message);
            return "failure";
        }
    }

    public void Stop()
    {
        Trace.WriteLine("[Networking] ClientServices.Stop() " +
            "function called.");
        //channel.Dispose();
    }

    /// <summary>
    /// Adds a module to the map of modules and notification handlers
    /// </summary>
    /// <param name="moduleName">Name of the module</param>
    /// <param name="notificationHandler">Notification handler of the module</param>
    /// <param name="isHighPriority">True if the module is high priority, else False</param>
    public void Subscribe(string moduleName, INotificationHandler
        notificationHandler, bool isHighPriority)
    {
        Trace.WriteLine("[Networking] " +
            "ClientServices.Subscribe() function called.");
        try
        {
            // store the notification handler of the module in our
            // map
            _moduleToNotificationHanderMap.Add(
                moduleName, notificationHandler);

            Trace.WriteLine("[Networking] Module: " + moduleName +
                " subscribed with priority [True for high/False" +
                "for low]: " + isHighPriority.ToString());
        }
        catch (Exception e)
        {
            Trace.WriteLine("[Networking] Error in " +
                "CommunicatorClient.Subscribe(): " + e.Message);
        }
    }

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
    /// Called remotely by the grpc client
    /// This function is called when a client sends data to the server
    /// The data is received by the server and passed to the appropriate module
    /// The module then calls the method "OnDataReceived" in the INotificationHandler
    /// </summary>
    /// <param name="request">Request containing the module name and serialized data</param>
    /// <param name="context">Context of the grpc call</param>
    /// <returns>Response containing the module name and serialized data</returns>
    public override Task<response> receive(request request, ServerCallContext context)
    {
        string moduleName = request.ModuleName;
        string serializedData = request.SerializedData;

        bool isModuleRegistered = false;
        lock (_maplock)
        {
            isModuleRegistered = _moduleToNotificationHanderMap.ContainsKey(moduleName);
        }
        if (!isModuleRegistered)
        {
            Trace.WriteLine($"[Networking] module {moduleName} does not have a handler.\n");
            return Task.FromResult(new response
            {

            });
        }

        INotificationHandler notificationHandler = null;

        // Getting the notification handler
        lock (_maplock)
        {
            notificationHandler = _moduleToNotificationHanderMap[moduleName];
        }

        // calling the method "OnDataReceived" on the handler of the appropriate module
        notificationHandler.OnDataReceived(serializedData);

        return Task.FromResult(new response
        {

        });
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

    /// <summary>
    /// assigns the client id to the client
    /// </summary>
    public override Task<setClientIdResponse> setclientid(setClientIdRequest request, ServerCallContext context)
    {
        _clientId = request.ClientId;
        return Task.FromResult(new setClientIdResponse
        {

        });
    }
}
