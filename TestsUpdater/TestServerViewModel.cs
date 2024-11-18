using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Networking.Communication;
using Updater;
using ViewModels.Updater;

namespace TestsUpdater;

/// <summary>
/// Unit tests for ServerViewModel.
/// Tests include validation for server data retrieval and broadcasting logic.
/// </summary>
[TestClass]
public class TestServerViewModel
{
    private Mock<LogServiceViewModel> _mockLogServiceViewModel;
    private Mock<ToolAssemblyLoader> _mockToolLoader;
    private Mock<ICommunicator> _mockCommunicator;
    private ServerViewModel _viewModel;

    private const string TestDirectory = @"C:\temp\test-tools";

    /// <summary>
    /// Setup method to initialize mock dependencies and the ServerViewModel before each test.
    /// </summary>
    [TestInitialize]
    public void Setup()
    {
        // Mock dependencies
        _mockLogServiceViewModel = new Mock<LogServiceViewModel>();
        _mockToolLoader = new Mock<ToolAssemblyLoader>();
        _mockCommunicator = new Mock<ICommunicator>();

        // Replace the _communicator field in the Server instance with the mocked communicator
        var server = Server.GetServerInstance();
        server._communicator = _mockCommunicator.Object;

        // Create test directory
        if (!Directory.Exists(TestDirectory))
        {
            Directory.CreateDirectory(TestDirectory);
        }

        // Initialize ServerViewModel with mocked dependencies
        _viewModel = new ServerViewModel(_mockLogServiceViewModel.Object, _mockToolLoader.Object, server);
    }

    /// <summary>
    /// Cleanup method to remove test directories after each test.
    /// </summary>
    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(TestDirectory))
        {
            Directory.Delete(TestDirectory, true);
        }
    }

    /// <summary>
    /// Test to verify that GetServerData returns valid JSON when the directory contains tool data.
    /// </summary>
    [TestMethod]
    public void TestGetServerData_ValidDirectory_ReturnsJson()
    {
        // Arrange
        var mockToolProperties = new Dictionary<string, List<string>>
        {
            { "Id", new List<string> { "1" } },
            { "Name", new List<string> { "Test Tool" } },
            { "Description", new List<string> { "A test tool description" } },
            { "Version", new List<string> { "1.0" } },
            { "LastUpdated", new List<string> { "2024-01-01" } },
            { "CreatorName", new List<string> { "Test Creator" } }
        };

        _mockToolLoader
            .Setup(loader => loader.LoadToolsFromFolder(It.IsAny<string>()))
            .Returns(mockToolProperties);

        // Act
        string result = _viewModel.GetServerData();

        // Assert
        Assert.IsFalse(string.IsNullOrWhiteSpace(result), "GetServerData should return a non-empty JSON string.");
        List<object>? deserializedResult = JsonSerializer.Deserialize<List<object>>(result);
        Assert.IsNotNull(deserializedResult, "The result should deserialize into a list of objects.");
        Assert.IsTrue(deserializedResult.Count > 0, "The result should contain at least one object.");
    }

    /// <summary>
    /// Test to verify that GetServerData returns an empty JSON array when the directory is invalid.
    /// </summary>
    [TestMethod]
    public void TestGetServerData_InvalidDirectory_ReturnsEmptyJson()
    {
        // Arrange
        _mockToolLoader
            .Setup(loader => loader.LoadToolsFromFolder(It.IsAny<string>()))
            .Throws(new IOException("Directory not found"));

        // Act
        string result = _viewModel.GetServerData();

        // Assert
        Assert.AreEqual("[]", result, "GetServerData should return an empty JSON array when the directory is invalid.");
    }

    /// <summary>
    /// Test to verify that BroadcastToClients broadcasts successfully when a valid file is provided.
    /// </summary>
    [TestMethod]
    public void TestBroadcastToClients_ValidFile_BroadcastsSuccessfully()
    {
        // Arrange
        string testFilePath = Path.Combine(TestDirectory, "testfile.txt");
        string testFileName = "testfile.txt";

        // Create a test file
        File.WriteAllText(testFilePath, "This is a test file.");

        _mockLogServiceViewModel
            .Setup(log => log.UpdateLogDetails(It.IsAny<string>()));

        _mockCommunicator
            .Setup(communicator => communicator.Send(It.IsAny<string>(), "FileTransferHandler", null))
            .Verifiable();

        // Act
        _viewModel.BroadcastToClients(testFilePath, testFileName);

        // Allow the background thread to complete
        Thread.Sleep(2000);

        // Assert
        _mockLogServiceViewModel.Verify(log => log.UpdateLogDetails("Sending files to all connected clients"), Times.Once);
        _mockCommunicator.Verify(communicator => communicator.Send(It.IsAny<string>(), "FileTransferHandler", null), Times.Once);

        string copiedFilePath = Path.Combine(AppConstants.ToolsDirectory, testFileName);
        Assert.IsTrue(File.Exists(copiedFilePath), "The file should be copied to the target directory.");

        // Cleanup copied file
        if (File.Exists(copiedFilePath))
        {
            File.Delete(copiedFilePath);
        }
    }

    /// <summary>
    /// Test to verify that BroadcastToClients throws a FileNotFoundException when the file is not found.
    /// </summary>
    [TestMethod]
    public void TestBroadcastToClients_FileNotFound_ThrowsFileNotFoundException()
    {
        // Arrange
        string invalidFilePath = Path.Combine(TestDirectory, "nonexistent.txt");
        string testFileName = "nonexistent.txt";

        // Act & Assert
        Assert.ThrowsException<FileNotFoundException>(() =>
            _viewModel.BroadcastToClients(invalidFilePath, testFileName),
            "BroadcastToClients should throw a FileNotFoundException if the file does not exist.");
    }
}
