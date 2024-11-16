/******************************************************************************
* Filename    = DirectoryMetadataComparer.cs
*
* Author      = Amithabh A
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = 
*****************************************************************************/

using System.Xml.Serialization;

namespace Updater
{
    [Serializable]
    [XmlRoot("ArrayOfMetadataDifference")]
    public class DirectoryMetadataComparer
    {
        [XmlElement("MetadataDifference")]
        public List<MetadataDifference> Differences { get; private set; } = new List<MetadataDifference>();

        // Properties for storing unique files in server and client directories
        [XmlElement("UniqueServerFiles")]
        public List<string> UniqueServerFiles { get; private set; } = new List<string>();
        [XmlElement("UniqueClientFiles")]

        public List<string> UniqueClientFiles { get; private set; } = new List<string>();
        public List<string> InvalidSyncUpFiles { get; private set; } = new List<string>();

        // Parameterless constructor for XML serialization
        public DirectoryMetadataComparer() { }

        /// <summary>
        /// Initialize new instance. 
        /// </summary>
        /// <param name="metadataA">Dir. A's metadata</param>
        /// <param name="metadataB">Dir. B's metadata</param>
        public DirectoryMetadataComparer(List<FileMetadata> metadataA, List<FileMetadata> metadataB)
        {
            CompareMetadata(metadataA, metadataB);
        }

        /// <summary>
        /// Compares and generates differences between a directory's metadata pairs.
        /// </summary>
        /// <param name="metadataA">Dir. A's metadata</param>
        /// <param name="metadataB">Dir. B's metadata</param>
        private void CompareMetadata(List<FileMetadata> metadataA, List<FileMetadata> metadataB)
        {
            // Initialize differences for three cases
            Differences.Add(new MetadataDifference { Key = "-1", Value = new List<FileDetail>() }); // In B but not in A
            Differences.Add(new MetadataDifference { Key = "0", Value = new List<FileDetail>() });  // Files with same hash but different names
            Differences.Add(new MetadataDifference { Key = "1", Value = new List<FileDetail>() });    // In A but not in B

            var hashToFileA = CreateHashToFileDictionary(metadataA);
            var hashToFileB = CreateHashToFileDictionary(metadataB);

            CheckForRenamesAndMissingFiles(metadataB, hashToFileA);
            CheckForOnlyInAFiles(metadataA, hashToFileB);
            CheckForSameNameDifferentHash(metadataA, metadataB);
        }

        /// <summary>
        /// Create a map from file hash to filename.
        /// </summary>
        /// <param name="metadata">List of metadata.</param>
        /// <returns>List of key-value pairs containing the mapping.</returns>
        private static List<KeyValuePair<string, string>> CreateHashToFileDictionary(List<FileMetadata> metadata)
        {
            return metadata.Select(file => new KeyValuePair<string, string>(file.FileHash, file.FileName)).ToList();
        }

        /// <summary>
        /// Checks for files in directory B that have been renamed or are missing in directory A.
        /// </summary>
        /// <param name="metadataB">Dir. B's metadata.</param>
        /// <param name="hashToFileA">List of key-value pairs mapping hash to filename in A.</param>
        private void CheckForRenamesAndMissingFiles(List<FileMetadata> metadataB, List<KeyValuePair<string, string>> hashToFileA)
        {
            foreach (var fileB in metadataB)
            {
                var existingFile = hashToFileA.FirstOrDefault(kvp => kvp.Key == fileB.FileHash);
                if (existingFile.Key != null)
                {
                    if (existingFile.Value != fileB.FileName)
                    {
                        Differences.First(d => d.Key == "0").Value.Add(new FileDetail
                        {
                            RenameFrom = fileB.FileName,
                            RenameTo = existingFile.Value,
                            FileHash = fileB.FileHash
                        });
                    }
                }
                else
                {
                    Differences.First(d => d.Key == "-1").Value.Add(new FileDetail
                    {
                        FileName = fileB.FileName,
                        FileHash = fileB.FileHash
                    });
                    UniqueClientFiles.Add(fileB.FileName);
                }
            }
        }

        /// <summary>
        /// Checks for files in directory A that are missing in directory B.
        /// </summary>
        /// <param name="metadataA">Dir. A's metadata</param>
        /// <param name="hashToFileB">List of key-value pairs mapping hash to filename in B.</param>
        private void CheckForOnlyInAFiles(List<FileMetadata> metadataA, List<KeyValuePair<string, string>> hashToFileB)
        {
            foreach (var fileA in metadataA)
            {
                if (!hashToFileB.Any(kvp => kvp.Key == fileA.FileHash))
                {
                    Differences.First(d => d.Key == "1").Value.Add(new FileDetail
                    {
                        FileName = fileA.FileName,
                        FileHash = fileA.FileHash
                    });
                    UniqueServerFiles.Add(fileA.FileName);
                }
            }
        }

        /// <summary>
        /// Checks for files in directory A and B that have the same name but different hashes.
        /// </summary>
        /// <param name="metadataA">Dir. A's metadata</param>
        /// <param name="metadataB">Dir. B's metadata</param>
        private void CheckForSameNameDifferentHash(List<FileMetadata> metadataA, List<FileMetadata> metadataB)
        {
            // Iterate over each file in directory A
            foreach (FileMetadata fileA in metadataA)
            {
                // Find a file in directory B that has the same name as fileA
                FileMetadata fileB = metadataB.FirstOrDefault(fb => fb.FileName == fileA.FileName);

                if (fileB != null && fileA.FileHash != fileB.FileHash)
                {
                    // Found files with the same name but different hashes
                    InvalidSyncUpFiles.Append(fileA.FileName);
                }
            }
        }

        /// <summary>
        /// Validates whether directories can be synced.
        /// </summary>
        public bool ValidateSync()
        {
            // If there are any filenames in InvalidSyncUpFiles, directories can't be synced. 
            return InvalidSyncUpFiles.Any();
        }
    }

    [Serializable]
    public class MetadataDifference
    {
        [XmlElement("Key")]
        public string Key { get; set; }

        [XmlArray("Value")]
        [XmlArrayItem("FileDetail")] // Define individual item
        public List<FileDetail> Value { get; set; } = new List<FileDetail>();
    }

    [Serializable]
    public class FileDetail
    {
        [XmlElement("FileName")]
        public string FileName { get; set; }

        [XmlElement("FileHash")]
        public string FileHash { get; set; }

        [XmlElement("RenameFrom")]
        public string RenameFrom { get; set; }

        [XmlElement("RenameTo")]
        public string RenameTo { get; set; }
    }
}
