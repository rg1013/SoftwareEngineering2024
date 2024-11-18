using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Updater;
using ViewModels.Updater;

namespace TestsUpdater;

[TestClass]
public class ToolListViewModelTests
{
    private Mock<IToolAssemblyLoader> _mockDllLoader;
    private ToolListViewModel _viewModel;
    // contains both v1 and v2 of a tool
    private string _testFolderPath = @"../../../TestingFolder";

    // contains v2 of a tool
    private string _copyTestFolderPath = @"../../../CopyTestFolder";

    [TestInitialize]
    public void Setup()
    {
        _mockDllLoader = new Mock<IToolAssemblyLoader>();
        string testFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _testFolderPath);
    }

    [TestMethod]
    public void TestLoadAvailableToolsShouldPopulateAvailableToolsListWhenToolsAreAvailable()
    {
        string copyTestFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _copyTestFolderPath);

        _viewModel = new ToolListViewModel(copyTestFolderPath);
        _viewModel.LoadAvailableTools(copyTestFolderPath);

        Assert.IsNotNull(_viewModel.AvailableToolsList);
        Assert.AreEqual(1, _viewModel.AvailableToolsList.Count);

        Tool tool = _viewModel.AvailableToolsList[0];
        Assert.AreEqual("OtherExample", tool.Name);
        Assert.AreEqual("2.0.0", tool.Version);
    }

    [TestMethod]
    public void TestLoadAvailableToolsShouldReplaceOlderVersionWhenNewerVersionExists()
    {
        // TestingFolder contains both v1 and v2 of the same Tool
        string testFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _testFolderPath);

        var viewModel = new ToolListViewModel(testFolderPath);

        viewModel.LoadAvailableTools(testFolderPath);
        ObservableCollection<Tool>? updatedTools = viewModel.AvailableToolsList;

        // Assert: Verify that the newer version replaced the older one
        Assert.AreEqual(1, updatedTools?.Count);
        Tool updatedTool = updatedTools.First();
        Assert.AreEqual("OtherExample", updatedTool.Name);
        Assert.AreEqual("2.0.0", updatedTool.Version);
    }


    [TestMethod]
    public void LoadAvailableTools_ShouldFirePropertyChanged_WhenToolsAreUpdated()
    {
        string testFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _testFolderPath);

        _viewModel = new ToolListViewModel(testFolderPath);

        bool wasPropertyChangedFired = false;
        _viewModel.PropertyChanged += (sender, e) => {
            if (e.PropertyName == nameof(_viewModel.AvailableToolsList))
            {
                wasPropertyChangedFired = true;
            }
        };

        _viewModel.LoadAvailableTools(testFolderPath);

        Assert.IsTrue(wasPropertyChangedFired);
    }
}
