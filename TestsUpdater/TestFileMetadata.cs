using Microsoft.VisualStudio.TestTools.UnitTesting;
using Updater;
namespace TestsUpdater;

[TestClass]
public class FileMetadataTests
{
    /// <summary>
    /// Verifies that the default constructor sets both FileName and FileHash to null.
    /// </summary>
    [TestMethod]
    public void Test_FileMetadata_DefaultConstructor()
    {
        // Arrange & Act
        var fileMetadata = new FileMetadata();

        // Assert
        Assert.IsNull(fileMetadata.FileName);
        Assert.IsNull(fileMetadata.FileHash);
    }

    /// <summary>
    /// Verifies that the constructor sets the FileName and FileHash correctly when valid values are provided.
    /// </summary>
    [TestMethod]
    public void Test_FileMetadata_Constructor_WithValidParams()
    {
        // Arrange
        var fileName = "example.txt";
        var fileHash = "abc123";

        // Act
        var fileMetadata = new FileMetadata {
            FileName = fileName,
            FileHash = fileHash
        };

        // Assert
        Assert.AreEqual(fileName, fileMetadata.FileName);
        Assert.AreEqual(fileHash, fileMetadata.FileHash);
    }

    /// <summary>
    /// Verifies that ToString() returns the correct format when both properties are null.
    /// </summary>
    [TestMethod]
    public void Test_FileMetadata_ToString_BothPropertiesNull()
    {
        // Arrange
        var fileMetadata = new FileMetadata();

        // Act
        var result = fileMetadata.ToString();

        // Assert
        Assert.AreEqual("FileName: N/A, FileHash: N/A", result);
    }

    /// <summary>
    /// Verifies that ToString() returns the correct format when FileName is null and FileHash is set.
    /// </summary>
    [TestMethod]
    public void Test_FileMetadata_ToString_FileNameNull_FileHashNotNull()
    {
        // Arrange
        var fileMetadata = new FileMetadata {
            FileName = null,
            FileHash = "abc123"
        };

        // Act
        var result = fileMetadata.ToString();

        // Assert
        Assert.AreEqual("FileName: N/A, FileHash: abc123", result);
    }

    /// <summary>
    /// Verifies that ToString() returns the correct format when FileName is set and FileHash is null.
    /// </summary>
    [TestMethod]
    public void Test_FileMetadata_ToString_FileNameNotNull_FileHashNull()
    {
        // Arrange
        var fileMetadata = new FileMetadata {
            FileName = "example.txt",
            FileHash = null
        };

        // Act
        var result = fileMetadata.ToString();

        // Assert
        Assert.AreEqual("FileName: example.txt, FileHash: N/A", result);
    }

    /// <summary>
    /// Verifies that ToString() returns the correct format when both FileName and FileHash are set.
    /// </summary>
    [TestMethod]
    public void Test_FileMetadata_ToString_BothPropertiesNotNull()
    {
        // Arrange
        var fileMetadata = new FileMetadata {
            FileName = "example.txt",
            FileHash = "abc123"
        };

        // Act
        var result = fileMetadata.ToString();

        // Assert
        Assert.AreEqual("FileName: example.txt, FileHash: abc123", result);
    }

    /// <summary>
    /// Verifies that FileName and FileHash handle empty strings correctly.
    /// </summary>
    [TestMethod]
    public void Test_FileMetadata_Constructor_EmptyStrings()
    {
        // Arrange
        var fileName = string.Empty;
        var fileHash = string.Empty;

        // Act
        var fileMetadata = new FileMetadata {
            FileName = fileName,
            FileHash = fileHash
        };

        // Assert
        Assert.AreEqual(fileName, fileMetadata.FileName);
        Assert.AreEqual(fileHash, fileMetadata.FileHash);
    }
}
