using Moq;
using Networking.Communication;
using ScreenShare;
using ScreenShare.Client;
using ScreenShare;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using SSUtils = ScreenShare.Utils;
using System.Windows.Controls;

namespace PlexShareTests.ScreenshareTests
{
    [Collection("Sequential")]
    public class ScreenshareClientTest
    {
        [Fact]
        public void TestSingleton()
        {
            ScreenshareClient screenshareClient = ScreenshareClient.GetInstance(isDebugging: true);
            Debug.Assert(screenshareClient != null);
        }

        [Fact]
        public void TestRegisterPacketSend()
        {
            // Arrange
            var communicatorMock = new Mock<ICommunicator>();
            string argString = "";
            communicatorMock.Setup(p => p.Send(It.IsAny<string>(), SSUtils.ModuleIdentifier, null))
                .Callback((string s, string s2, string s3) => { if (argString == "") argString = s; });

            var screenshareClient = ScreenshareClient.GetInstance(isDebugging: true);
            typeof(ScreenshareClient).GetField("_communicator", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(screenshareClient, communicatorMock.Object);

            screenshareClient.SetUser("id", "name");

            // Act
            screenshareClient.StartScreensharing();
            DataPacket? packet = JsonSerializer.Deserialize<DataPacket>(argString);

            // Assert
            Assert.True(packet?.Header == ClientDataHeader.Register.ToString());
            communicatorMock.Verify(p => p.Send(It.IsAny<string>(), SSUtils.ModuleIdentifier, null), Times.AtLeastOnce);
        }



        [Fact]
        public void TestSendPacketReceive()
        {
            // Arrange
            var communicatorMock = new Mock<ICommunicator>();
            bool isImagePacketSent = false;
            communicatorMock.Setup(p => p.Send(It.IsAny<string>(), SSUtils.ModuleIdentifier, null))
                .Callback((string s, string s2, string s3) =>
                {
                    DataPacket? packet = JsonSerializer.Deserialize<DataPacket>(s);
                    if (packet?.Header == ClientDataHeader.Image.ToString())
                    {
                        isImagePacketSent = true;
                    }
                });

            var screenshareClient = ScreenshareClient.GetInstance(isDebugging: true);
            typeof(ScreenshareClient).GetField("_communicator", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(screenshareClient, communicatorMock.Object);

            screenshareClient.SetUser("id", "name");
            screenshareClient.StartScreensharing();

            DataPacket packet = new("id", "name", ServerDataHeader.Send.ToString(), "10");
            string serializedData = JsonSerializer.Serialize(packet);

            // Act
            screenshareClient.OnDataReceived(serializedData);
            Thread.Sleep(1000);  // Optional: wait for async operations if needed

            // Assert
            Assert.True(isImagePacketSent);
            communicatorMock.Verify(p => p.Send(It.IsAny<string>(), SSUtils.ModuleIdentifier, null), Times.AtLeastOnce);
            screenshareClient.StopScreensharing();
        }


        [Fact]
        public void TestStopPacketReceive()
        {
            // Arrange
            var communicatorMock = new Mock<ICommunicator>();
            bool isImagePacketSent = false;
            communicatorMock.Setup(p => p.Send(It.IsAny<string>(), SSUtils.ModuleIdentifier, null))
                .Callback((string s, string s2, string s3) =>
                {
                    DataPacket? packet = JsonSerializer.Deserialize<DataPacket>(s);
                    if (packet?.Header == ClientDataHeader.Image.ToString())
                    {
                        isImagePacketSent = true;
                    }
                });

            var screenshareClient = ScreenshareClient.GetInstance(isDebugging: true);
            typeof(ScreenshareClient).GetField("_communicator", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(screenshareClient, communicatorMock.Object);

            screenshareClient.SetUser("id", "name");
            screenshareClient.StartScreensharing();

            // Simulate receiving a send packet
            DataPacket packet = new("id", "name", ServerDataHeader.Send.ToString(), "10");
            string serializedData = JsonSerializer.Serialize(packet);
            screenshareClient.OnDataReceived(serializedData);

            Thread.Sleep(1000);
            Assert.True(isImagePacketSent);

            // Simulate receiving a stop packet
            packet = new("id", "name", ServerDataHeader.Stop.ToString(), "10");
            serializedData = JsonSerializer.Serialize(packet);
            screenshareClient.OnDataReceived(serializedData);

            Thread.Sleep(1000);

            bool? _imageCancellationToken = (bool?)typeof(ScreenshareClient)
                .GetField("_imageCancellationToken", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(screenshareClient);

            // Assert
            Assert.True(_imageCancellationToken);
            communicatorMock.Verify(p => p.Send(It.IsAny<string>(), SSUtils.ModuleIdentifier, null), Times.AtLeastOnce);
            screenshareClient.StopScreensharing();
        }



    }
}