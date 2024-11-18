using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Updater;

namespace TestsUpdater;

[TestClass]
public class TestDirectoryMetadataComparer
{
    private List<FileMetadata> metadataA;
    private List<FileMetadata> metadataB;

    [TestInitialize]
    public void Setup()
    {
        // Prepare some sample metadata for testing
        metadataA = new List<FileMetadata>
        {
            new FileMetadata { FileName = "file1.txt", FileHash = "hash1" },
            new FileMetadata { FileName = "file2.txt", FileHash = "hash2" },
            new FileMetadata { FileName = "file5.txt", FileHash = "hash5" },
        };

        metadataB = new List<FileMetadata>
        {
            new FileMetadata { FileName = "file2.txt", FileHash = "hash2" },
            new FileMetadata { FileName = "file3.txt", FileHash = "hash3" },
            new FileMetadata { FileName = "file4.txt", FileHash = "hash1" },
        };
    }

    [TestMethod]
    public void CompareMetadata_ShouldIdentifyDifferences()
    {
        // Arrange
        var comparer = new DirectoryMetadataComparer(metadataA, metadataB);

        // Act
        var differences = comparer.Differences;

        // Assert: Check if differences are as expected
        Assert.AreEqual(1, differences.First(d => d.Key == "-1").Value.Count);
        Assert.AreEqual(1, differences.First(d => d.Key == "0").Value.Count);
        Assert.AreEqual(1, differences.First(d => d.Key == "1").Value.Count);
    }

    [TestMethod]
    public void CheckForRenamesAndMissingFiles_ShouldIdentifyMissingFilesInA()
    {
        // Arrange
        var comparer = new DirectoryMetadataComparer(metadataA, metadataB);

        // Act
        var uniqueClientFiles = comparer.UniqueClientFiles;

        // Assert: Check if missing files from A are identified
        Assert.AreEqual(1, uniqueClientFiles.Count); // file4.txt is only in B
        Assert.AreEqual("file3.txt", uniqueClientFiles.First());
    }

    [TestMethod]
    public void CheckForOnlyInAFiles_ShouldIdentifyMissingFilesInB()
    {
        // Arrange
        var comparer = new DirectoryMetadataComparer(metadataA, metadataB);

        // Act
        var uniqueServerFiles = comparer.UniqueServerFiles;

        // Assert: Check if missing files from B are identified
        Assert.AreEqual(1, uniqueServerFiles.Count); // file1.txt is only in A
        Assert.AreEqual("file5.txt", uniqueServerFiles.First());
    }

    [TestMethod]
    public void CheckForSameNameDifferentHash_ShouldIdentifyFileHashMismatch()
    {
        // Arrange
        var comparer = new DirectoryMetadataComparer(metadataA, metadataB);

        // Act
        var invalidSyncUpFiles = comparer.InvalidSyncUpFiles;

        Assert.AreEqual(0, invalidSyncUpFiles.Count);
    }

    [TestMethod]
    public void ValidateSync_ShouldReturnTrue_WhenNoInvalidFilesExist()
    {
        // Arrange
        var comparer = new DirectoryMetadataComparer(metadataA, metadataB);

        // Act
        var canSync = comparer.ValidateSync();

        // Assert: Should return true due to file3.txt having different hashes
        Assert.IsTrue(canSync);
    }

    [TestMethod]
    public void ValidateSync_ShouldReturnFalse_WhenNoInvalidFiles()
    {
        // Arrange
        var metadataBInvalidFiles = new List<FileMetadata>
        {
            new FileMetadata { FileName = "file1.txt", FileHash = "hash1" },
            new FileMetadata { FileName = "file2.txt", FileHash = "hash2" },
            new FileMetadata { FileName = "file5.txt", FileHash = "hash7" }
        };
        var comparer = new DirectoryMetadataComparer(metadataA, metadataBInvalidFiles);

        // Act
        var canSync = comparer.ValidateSync();

        // Assert: Should return false as there are no invalid files
        Assert.IsFalse(canSync);
    }
}
