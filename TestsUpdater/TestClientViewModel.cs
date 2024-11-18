﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            null, [], null);

        if (constructorInfo == null)
        {
            Assert.Fail("Unable to find a suitable constructor for Client.");
        }

        _mockClient = (Client)constructorInfo.Invoke(null);

        // Replace the LogServiceViewModel with a mocked one in Client
        typeof(Client)
            .GetField("OnLogUpdate", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
            ?.SetValue(null, (Action<string>)_mockLogServiceViewModel.Object.UpdateLogDetails);

        // Initializing ClientViewModel and inject the mock LogServiceViewModel
        _viewModel = new ClientViewModel(_mockLogServiceViewModel.Object);

        // Injecting the private _client field in ClientViewModel with our mock
        typeof(ClientViewModel)
            .GetField("_client", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(_viewModel, _mockClient);
    }

    [TestMethod]
    public async Task TestSyncUpAsync_InvokesClientAndLogsCompletion()
    {
        bool syncUpCalled = false;

        // Using reflection to mock SyncUp method behavior
        typeof(Client)
            .GetMethod("SyncUp", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
            ?.Invoke(_mockClient, []);

        if (_mockLogServiceViewModel == null)
        {
            Assert.Fail("_mockLogServiceViewModel is not initialized.");
        }


        // Update LogServiceViewModel mock
        _mockLogServiceViewModel
            .Setup(log => log.UpdateLogDetails("Sync completed."))
            .Callback(() => syncUpCalled = true)
            .Verifiable();

        if (_viewModel == null)
        {
            Assert.Fail("_viewModel is not initialized.");
        }

        await _viewModel.SyncUpAsync();


        Assert.IsTrue(syncUpCalled, "SyncUp should have been called.");
        _mockLogServiceViewModel.Verify(
            log => log.UpdateLogDetails("Sync completed."),
            Times.Once,
            "LogServiceViewModel.UpdateLogDetails should be called with 'Sync completed.'"
        );
    }
}
