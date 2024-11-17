using ViewModels.Updater;

namespace TestsUpdater;

[TestClass]
public class TestTool
{
    private Tool? _tool;

    [TestInitialize]
    public void Setup()
    {
        _tool = new Tool();
    }

    [TestMethod]
    public void TestIdShouldRaisePropertyChanged()
    {
        bool eventRaised = false;
        _tool.PropertyChanged += (sender, e) => {
            if (e.PropertyName == nameof(Tool.ID))
            {
                eventRaised = true;
            }
        };

        _tool.ID = "123";

        Assert.IsTrue(eventRaised, "PropertyChanged event was not raised for ID.");
        Assert.AreEqual("123", _tool.ID, "ID was not set correctly.");
    }

    [TestMethod]
    public void TestVersionShouldRaisePropertyChanged()
    {
        bool eventRaised = false;
        _tool.PropertyChanged += (sender, e) => {
            if (e.PropertyName == nameof(Tool.Version))
            {
                eventRaised = true;
            }
        };

        _tool.Version = "1.0.0";

        Assert.IsTrue(eventRaised, "PropertyChanged event was not raised for Version.");
        Assert.AreEqual("1.0.0", _tool.Version, "Version was not set correctly.");
    }

    [TestMethod]
    public void TestDescriptionShouldRaisePropertyChanged()
    {
        bool eventRaised = false;
        _tool.PropertyChanged += (sender, e) => {
            if (e.PropertyName == nameof(Tool.Description))
            {
                eventRaised = true;
            }
        };

        _tool.Description = "Sample tool description.";

        Assert.IsTrue(eventRaised, "PropertyChanged event was not raised for Description.");
        Assert.AreEqual("Sample tool description.", _tool.Description, "Description was not set correctly.");
    }

    [TestMethod]
    public void TestDeprecatedShouldRaisePropertyChanged()
    {
        bool eventRaised = false;
        _tool.PropertyChanged += (sender, e) => {
            if (e.PropertyName == nameof(Tool.Deprecated))
            {
                eventRaised = true;
            }
        };

        _tool.Deprecated = "Yes";

        Assert.IsTrue(eventRaised, "PropertyChanged event was not raised for Deprecated.");
        Assert.AreEqual("Yes", _tool.Deprecated, "Deprecated was not set correctly.");
    }

    [TestMethod]
    public void TestCreatedByShouldRaisePropertyChanged()
    {
        bool eventRaised = false;
        _tool.PropertyChanged += (sender, e) => {
            if (e.PropertyName == nameof(Tool.CreatedBy))
            {
                eventRaised = true;
            }
        };

        _tool.CreatedBy = "Jane Doe";

        Assert.IsTrue(eventRaised, "PropertyChanged event was not raised for CreatedBy.");
        Assert.AreEqual("Jane Doe", _tool.CreatedBy, "CreatedBy was not set correctly.");
    }

    [TestMethod]
    public void TestCreatorEmailShouldRaisePropertyChanged()
    {
        bool eventRaised = false;
        _tool.PropertyChanged += (sender, e) => {
            if (e.PropertyName == nameof(Tool.CreatorEmail))
            {
                eventRaised = true;
            }
        };

        _tool.CreatorEmail = "janedoe@example.com";

        Assert.IsTrue(eventRaised, "PropertyChanged event was not raised for CreatorEmail.");
        Assert.AreEqual("janedoe@example.com", _tool.CreatorEmail, "CreatorEmail was not set correctly.");
    }

    [TestMethod]
    public void TestLastUpdatedShouldRaisePropertyChanged()
    {
        bool eventRaised = false;
        _tool.PropertyChanged += (sender, e) => {
            if (e.PropertyName == nameof(Tool.LastUpdated))
            {
                eventRaised = true;
            }
        };

        _tool.LastUpdated = "2024-11-17";

        Assert.IsTrue(eventRaised, "PropertyChanged event was not raised for LastUpdated.");
        Assert.AreEqual("2024-11-17", _tool.LastUpdated, "LastUpdated was not set correctly.");
    }

    [TestMethod]
    public void TestLastModifiedShouldRaisePropertyChanged()
    {
        bool eventRaised = false;
        _tool.PropertyChanged += (sender, e) => {
            if (e.PropertyName == nameof(Tool.LastModified))
            {
                eventRaised = true;
            }
        };

        _tool.LastModified = "2024-11-16";

        Assert.IsTrue(eventRaised, "PropertyChanged event was not raised for LastModified.");
        Assert.AreEqual("2024-11-16", _tool.LastModified, "LastModified was not set correctly.");
    }

    [TestMethod]
    public void TestPropertyChangedEventIsNotRaisedWhenSettingSameValue()
    {
        _tool.ID = "123"; // Setting initial value
        bool eventRaised = false;
        _tool.PropertyChanged += (sender, e) => {
            if (e.PropertyName == nameof(Tool.ID))
            {
                eventRaised = true;
            }
        };
        _tool.ID = "123"; // Setting the same value again

        Assert.IsFalse(eventRaised, "PropertyChanged event should not be raised when setting the same value.");
    }
}
