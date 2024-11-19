using System.Diagnostics;
using Updater;
using Networking;
using Networking.Communication;


namespace TestsUpdater;

public class StringWriterTraceListener : TraceListener
{
    private StringWriter _stringWriter;

    public StringWriterTraceListener(StringWriter stringWriter)
    {
        _stringWriter = stringWriter ?? throw new ArgumentNullException(nameof(stringWriter));
    }

    public override void Write(string message)
    {
        _stringWriter.Write(message);
    }

    public override void WriteLine(string message)
    {
        _stringWriter.WriteLine(message);
    }

    public string GetOutput()
    {
        return _stringWriter.ToString();
    }
}

[TestClass]
public class ClientTests
{
    private static ICommunicator? s_communicator;
    private static Client? s_client;
    private static StringWriter? s_traceOutput;

    // Test initialization
    [ClassInitialize]
    public static void ClassInit(TestContext? context)
    {
        s_communicator = CommunicationFactory.GetCommunicator(isClientSide: true);
        s_client = Client.GetClientInstance(notificationReceived: (message) => Trace.WriteLine(message));

        s_traceOutput = new StringWriter();
        Trace.Listeners.Clear();
        Trace.Listeners.Add(new TextWriterTraceListener(s_traceOutput));
    }

    [TestInitialize]
    public void TestInitialize()
    {
        s_traceOutput?.GetStringBuilder().Clear();
    }

    [TestMethod]
    public void TestGetClientInstance_Singleton()
    {
        var client1 = Client.GetClientInstance();
        var client2 = Client.GetClientInstance();
        Assert.AreEqual(client1, client2, "Client instance is not singleton");
    }

    [TestMethod]
    public void TestSyncUp_Success()
    {
        s_client?.GetClientId("TestClient123");
        s_client?.SyncUp();

        Assert.IsTrue(s_traceOutput?.ToString().Contains("Sending syncup request to the server"),
                      "SyncUp did not log expected message");
    }

    [TestMethod]
    public void TestStop()
    {
        s_client.Stop();
        Assert.IsTrue(s_traceOutput?.ToString().Contains("Client disconnected"),
                      "Stop method did not log expected message");
    }

    [TestMethod]
    public void TestPacketDemultiplexer_SyncUp()
    {
        var dataPacket = new DataPacket(DataPacket.PacketType.SyncUp, new List<FileContent>());
        string serializedData = Utils.SerializeObject(dataPacket)!;

        if (s_communicator != null)
        {
            Client.PacketDemultiplexer(serializedData, s_communicator);

            Assert.IsTrue(s_traceOutput?.ToString().Contains("Received SyncUp request from server"),
                          "SyncUpHandler not called correctly");
        }
    }

    [TestMethod]
    public void TestSyncUpHandler()
    {
        var syncUpPacket = new DataPacket(DataPacket.PacketType.SyncUp, new List<FileContent>());
        if (s_communicator != null)
        {
            Client.SyncUpHandler(syncUpPacket, s_communicator);

            Assert.IsTrue(s_traceOutput?.ToString().Contains("Metadata sent to server"),
                          "SyncUpHandler did not send metadata");
        }
    }

    [TestMethod]
    public void TestInvalidSyncHandler()
    {
        var fileContent = new FileContent("invalid.txt", Utils.SerializeObject(new List<string> { "file1.txt" })!);
        var dataPacket = new DataPacket(DataPacket.PacketType.InvalidSync, new List<FileContent> { fileContent });

        if (s_communicator != null)
        {
            Client.InvalidSyncHandler(dataPacket, s_communicator);

            Assert.IsTrue(s_traceOutput?.ToString().Contains("Received invalid file names from server"),
                          "InvalidSyncHandler did not log expected message");
        }
    }

    [TestMethod]
    public void TestBroadcastHandler()
    {
        var fileContent = new FileContent("test.txt", Utils.SerializeObject("test content")!);
        var dataPacket = new DataPacket(DataPacket.PacketType.Broadcast, new List<FileContent> { fileContent });

        if (s_communicator != null)
        {
            Client.BroadcastHandler(dataPacket, s_communicator);

            Assert.IsTrue(s_traceOutput?.ToString().Contains("Up-to-date with the server"),
                          "BroadcastHandler did not update correctly");
        }
    }

    [TestMethod]
    public void TestDifferencesHandler()
    {
        var diffContent = new FileContent("differences", Utils.SerializeObject(new List<MetadataDifference>())!);
        var fileContent = new FileContent("file.txt", Utils.SerializeObject("content")!);
        var dataPacket = new DataPacket(DataPacket.PacketType.Differences, new List<FileContent> { diffContent, fileContent });

        if (s_communicator != null)
        {
            Client.DifferencesHandler(dataPacket, s_communicator);

            Assert.IsTrue(s_traceOutput?.ToString().Contains("Sending requested files to server"),
                          "DifferencesHandler did not send files correctly");
        }
    }

    [TestMethod]
    public void TestOnDataReceived()
    {
        var dataPacket = new DataPacket(DataPacket.PacketType.SyncUp, new List<FileContent>());
        string serializedData = Utils.SerializeObject(dataPacket)!;

        s_client?.OnDataReceived(serializedData);

        Assert.IsTrue(s_traceOutput?.ToString().Contains("FileTransferHandler received data"),
                      "OnDataReceived did not handle packet correctly");
    }

    [TestMethod]
    public void TestShowInvalidFilesInUI()
    {
        Client.ShowInvalidFilesInUI(new List<string> { "file1.txt", "file2.txt" });
        Assert.IsTrue(s_traceOutput?.ToString().Contains("Invalid filenames"),
                      "ShowInvalidFilesInUI did not log expected message");
    }
}
