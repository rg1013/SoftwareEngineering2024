using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ViewModels.Updater;
using SECloud.Services;
using Networking.Communication;
using System.Linq;
using Moq;
using SECloud.Models;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using System.Security.Policy;
using Updater;

namespace TestsUpdater;

[TestClass]
public class CloudViewModelTests
{
    private Mock<CloudViewModel> _cloudViewModel;
    private Mock<LogServiceViewModel> _mockLogServiceViewModel;
    private Mock<ServerViewModel> _mockServerViewModel;
    private Mock<CloudService> _cloudService;
    private Mock<ToolAssemblyLoader> _loader;
    private Mock<Server> _server;
    [TestInitialize]
    public void Setup()
    {
        // Mock the dependencies
        _mockLogServiceViewModel = new Mock<LogServiceViewModel>();
        _cloudService = new Mock<CloudService>();
        _server = new Mock<Server>();
        // Create an instance of CloudViewModel with mocked dependencies
        _cloudViewModel = new Mock<CloudViewModel>(_mockLogServiceViewModel, _mockServerViewModel);
        _loader = new Mock<ToolAssemblyLoader>();
        _mockServerViewModel = new Mock<ServerViewModel>(_mockLogServiceViewModel, _loader, _server);
    }

    [TestMethod]
    public void TestRemoveNAEntries_RemovesInvalidEntries()
    {
        // Arrange
        var files = new List<CloudViewModel.FileData>
        {
            new CloudViewModel.FileData { Name = new List<string> { "ValidFile" }, Id = new List<string> { "1" } },
            new CloudViewModel.FileData { Name = new List<string> { "N/A" }, Id = new List<string> { "2" } },
            new CloudViewModel.FileData { Name = new List<string> { "AnotherValidFile" }, Id = new List<string> { "N/A" } }
        };

        // Act
        List<CloudViewModel.FileData> result = CloudViewModel.RemoveNAEntries(files);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("ValidFile", result[0].Name[0]);
    }
    [TestMethod]
    public void TestServerHasMoreData_IdentifiesServerOnlyFiles()
    {
        // Arrange
        string cloudData = "[]"; // Empty cloud
        string serverData = "[{\"Name\": [\"ServerFile\"], \"Id\": [\"1\"]}]";

        // Act
        List<CloudViewModel.FileData> result = CloudViewModel.ServerHasMoreData(cloudData, serverData);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("ServerFile", result[0].Name[0]);
        Assert.AreEqual("1", result[0].Id[0]);
    }


    [TestMethod]
    public void TestCloudHasMoreData_IdentifiesCloudOnlyFiles()
    {
        // Arrange
        string serverData = "[]"; // Empty server
        string cloudData = "[{\"Name\": [\"CloudFile\"], \"Id\": [\"2\"]}]";

        // Act
        List<CloudViewModel.FileData> result = CloudViewModel.CloudHasMoreData(cloudData, serverData);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("CloudFile", result[0].Name[0]);
        Assert.AreEqual("2", result[0].Id[0]);
    }

    [TestMethod]
    public void ServerHasMoreData_FiltersCorrectFiles()
    {
        // Arrange: Setup mock data for cloud and server
        string cloudData = "[{\"Id\": [\"1\"], \"Name\": [\"File1\"], \"FileVersion\": [\"1.0\"]}]";
        string serverData = "[{\"Id\": [\"2\"], \"Name\": [\"File2\"], \"FileVersion\": [\"1.0\"]}]";

        // Act: Find files missing in the cloud
        List<CloudViewModel.FileData> result = CloudViewModel.ServerHasMoreData(cloudData, serverData);

        // Assert: Ensure the correct files are identified for upload to the cloud
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("File2", result[0].Name[0]);
    }

    [TestMethod]
    public void CloudHasMoreData_FiltersCorrectFiles()
    {
        // Arrange: Setup mock data for cloud and server
        string cloudData = "[{\"Id\": [\"1\"], \"Name\": [\"File1\"], \"FileVersion\": [\"1.0\"]}]";
        string serverData = "[{\"Id\": [\"1\"], \"Name\": [\"File1\"], \"FileVersion\": [\"1.0\"]}]";

        // Act: Find files unique to the cloud
        List<CloudViewModel.FileData> result = CloudViewModel.CloudHasMoreData(cloudData, serverData);

        // Assert: Ensure there are no extra cloud files (as both cloud and server have the same data)
        Assert.AreEqual(0, result.Count);
    }
}
