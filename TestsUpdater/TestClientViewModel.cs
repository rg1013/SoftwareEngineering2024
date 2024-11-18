using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using Updater;
using ViewModels.Updater;

namespace TestsUpdater;

[TestClass]
public class TestClientViewModel
{
    private Mock<LogServiceViewModel>? _mockLogServiceViewModel;
    private ClientViewModel? _viewModel;
    private Client? _mockClient;

    [TestInitialize]
    public void Setup()
    {
        // Mock LogServiceViewModel
        _mockLogServiceViewModel = new Mock<LogServiceViewModel>();

        // Use reflection to instantiate the Client class
        System.Reflection.ConstructorInfo? constructorInfo = typeof(Client).GetConstructor(
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
            null, new Type[] { }, null);

        _mockClient = (Client)constructorInfo?.Invoke(null);

        // Replace the LogServiceViewModel with a mocked one in Client
        typeof(Client)
            .GetField("OnLogUpdate", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
            ?.SetValue(null, (Action<string>)_mockLogServiceViewModel.Object.UpdateLogDetails);

        // Initialize ClientViewModel and inject the mock LogServiceViewModel
        _viewModel = new ClientViewModel(_mockLogServiceViewModel.Object);

        // Inject the private _client field in ClientViewModel with our mock
        typeof(ClientViewModel)
            .GetField("_client", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(_viewModel, _mockClient);
    }

    [TestMethod]
    public async Task TestSyncUpAsync_InvokesClientAndLogsCompletion()
    {
        // Arrange
        bool syncUpCalled = false;

        // Use reflection to mock SyncUp method behavior
        typeof(Client)
            .GetMethod("SyncUp", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
            ?.Invoke(_mockClient, new object[] { });

        // Update LogServiceViewModel mock
        _mockLogServiceViewModel
            .Setup(log => log.UpdateLogDetails("Sync completed."))
            .Callback(() => syncUpCalled = true)
            .Verifiable();

        // Act
        await _viewModel.SyncUpAsync();

        // Assert
        Assert.IsTrue(syncUpCalled, "SyncUp should have been called.");
        _mockLogServiceViewModel.Verify(
            log => log.UpdateLogDetails("Sync completed."),
            Times.Once,
            "LogServiceViewModel.UpdateLogDetails should be called with 'Sync completed.'"
        );
    }
}
