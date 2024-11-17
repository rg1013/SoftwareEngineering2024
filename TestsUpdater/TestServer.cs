using Moq;
using Networking.Communication;
using Updater;
using Networking;
using System.Reflection;
using System.Net.Sockets;

namespace TestsUpdater;

[TestClass]
public class TestServer
{
    private Mock<ICommunicator>? _mockCommunicator;
    private Server? _server;

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

        // Use reflection to set the private field or property where the communicator is set
        FieldInfo? communicatorField = typeof(Server).GetField("_communicator", BindingFlags.NonPublic | BindingFlags.Instance);
        communicatorField.SetValue(_server, _mockCommunicator.Object);
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
    public void TestBroadcastShouldInvokeCommunicatorSend()
    {
        // Arrange
        string serializedPacket = "test_packet";
        _mockCommunicator?.Setup(m => m.Send(serializedPacket, "FileTransferHandler", null));

        // Act
        _server?.Broadcast(serializedPacket);

        // Allow for any async behavior (if needed)
        Thread.Sleep(100); // Adjust delay if necessary for async operations

        // Assert
        _mockCommunicator?.Verify(m => m.Send(serializedPacket, "FileTransferHandler", null), Times.Once);
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
            _server.RequestSyncUp(clientId);
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
    public void UpdateUILogs_ShouldInvokeNotificationReceivedEvent()
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
        typeof(Server)
            .GetMethod("PacketDemultiplexer", BindingFlags.NonPublic | BindingFlags.Static)?
            .Invoke(null, new object[]
            {
                serializedData,
                _mockCommunicator?.Object!,
                _server,
                "client123"
            });

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
}
