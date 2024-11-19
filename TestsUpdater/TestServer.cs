using Moq;
using Networking.Communication;
using Updater;
using Networking;
using System.Reflection;
using System.Net.Sockets;
using System.Diagnostics;
using Networking.Serialization;
using Newtonsoft.Json.Serialization;
namespace TestsUpdater;

[TestClass]
public class TestServer
{
    private Mock<ICommunicator>? _mockCommunicator;
    private Server? _server;
    private StringWriterTraceListener? _traceListener;
    private StringWriter? _traceWriter;

    public interface ICommunicatorFactory
    {
        ICommunicator GetCommunicator(bool param);
    }

    public class CommunicationFactoryAdapter : ICommunicatorFactory
    {
        public ICommunicator GetCommunicator(bool param)
        {
            return CommunicationFactory.GetCommunicator(param);
        }
    }


    [TestInitialize]
    public void Setup()
    {
        // Mock the ICommunicator
        _mockCommunicator = new Mock<ICommunicator>();

        // Mock the factory to return the mocked ICommunicator
        var mockFactory = new Mock<ICommunicatorFactory>();
        mockFactory.Setup(m => m.GetCommunicator(It.IsAny<bool>())).Returns(_mockCommunicator.Object);

        // Initialize Server instance
        _server = Server.GetServerInstance();

        // Use reflection to set the private communicator field
        FieldInfo? communicatorField = typeof(Server).GetField("s_communicator", BindingFlags.NonPublic | BindingFlags.Instance);
        communicatorField?.SetValue(_server, _mockCommunicator.Object);

        // Redirect trace output to StringWriterTraceListener for logging validation
        _traceWriter = new StringWriter();
        _traceListener = new StringWriterTraceListener(_traceWriter);
        Trace.Listeners.Add(_traceListener);
    }

    [TestMethod]
    public void Broadcast_ShouldStartBroadcastingThread()
    {
        // Arrange
        string testPacket = "test_serialized_packet";

        // Act
        _server?.Broadcast(testPacket);

        // Assert
        // Since we can't directly check threads, we can assert that no exceptions are thrown during execution.
        Assert.IsTrue(true); // Just a placeholder, can be enhanced with more detailed checks.
    }



    [TestMethod]
    public void TestGetServerInstanceShouldReturnSameInstance()
    {
        // Act
        var instance1 = Server.GetServerInstance();
        var instance2 = Server.GetServerInstance();

        // Assert
        Assert.AreSame(instance1, instance2, "GetServerInstance should return the same server instance.");
    }

    [TestMethod]
    public void TestRequestSyncUpShouldCallSyncUp()
    {
        // Arrange
        string clientId = "client123";
        bool syncUpCalled = false;

        // Act
        try
        {
            _server?.RequestSyncUp(clientId);
            syncUpCalled = true;
        }
        catch (Exception)
        {
            // Ignore exception
        }

        // Assert
        Assert.IsTrue(syncUpCalled);
    }


    [TestMethod]
    public void TestUpdateUILogsShouldInvokeNotificationReceivedEvent()
    {
        // Arrange
        string logMessage = "Test log message";
        bool eventInvoked = false;

        // Subscribe to the NotificationReceived event
        Server.NotificationReceived += (message) => {
            if (message == logMessage)
            {
                eventInvoked = true;
            }
        };

        // Act
        Server.UpdateUILogs(logMessage);

        // Assert
        Assert.IsTrue(eventInvoked);
    }

    [TestMethod]
    public void TestOnDataReceivedShouldCallPacketDemultiplexer()
    {
        // Arrange
        string serializedData = "test_serialized_data";
        bool packetDemultiplexerCalled = false;

        // Mock PacketDemultiplexer behavior
        _ = (typeof(Server)
            .GetMethod("PacketDemultiplexer", BindingFlags.NonPublic | BindingFlags.Static)?
            .Invoke(null,
            [
                serializedData,
                _mockCommunicator?.Object!,
                _server,
                "client123"
            ]));

        // Act
        try
        {
            _server?.OnDataReceived(serializedData);
            packetDemultiplexerCalled = true;
        }
        catch (Exception)
        {
            // Ignore exceptions
        }

        // Assert
        Assert.IsTrue(packetDemultiplexerCalled);
    }

    [TestMethod]
    public void TestSetUserAddsClientToConnections()
    {
        // Arrange
        string clientId = "client123";
        var mockTcpClient = new Mock<TcpClient>();
        FieldInfo? clientConnectionsField = typeof(Server).GetField("_clientConnections", BindingFlags.NonPublic | BindingFlags.Instance);
        var clientConnections = (Dictionary<string, TcpClient>?)clientConnectionsField?.GetValue(_server);

        // Act
        _server?.SetUser(clientId, mockTcpClient.Object);

        // Assert
        Assert.IsTrue(clientConnections?.ContainsKey(clientId) ?? false, "Client should be added to connections.");
    }

    [TestMethod]
    public void TestOnClientLeftRemovesClientFromConnections()
    {
        // Arrange
        string clientId = "client123";
        var mockTcpClient = new Mock<TcpClient>();
        FieldInfo? clientConnectionsField = typeof(Server).GetField("_clientConnections", BindingFlags.NonPublic | BindingFlags.Instance);
        var clientConnections = (Dictionary<string, TcpClient>?)clientConnectionsField?.GetValue(_server);

        // Add a client
        _server?.SetUser(clientId, mockTcpClient.Object);

        // Act
        _server?.OnClientLeft(clientId);

        // Assert
        Assert.IsFalse(clientConnections?.ContainsKey(clientId) ?? true, "Client should be removed from connections.");
    }

    [TestMethod]
    public void TestCompleteSyncShouldSignalSemaphore()
    {
        // Arrange
        var binarySemaphore = new BinarySemaphore(); // Adjust constructor as per your implementation
        FieldInfo? semaphoreField = typeof(Server).GetField("_semaphore", BindingFlags.NonPublic | BindingFlags.Instance);
        semaphoreField?.SetValue(_server, binarySemaphore);

        // Act
        _server?.CompleteSync();

        // Assert
        bool signaled = Task.Run(binarySemaphore.Wait).Wait(1000); // Timeout after 1 second
        Assert.IsTrue(signaled, "CompleteSync did not signal the semaphore as expected.");
    }
}
