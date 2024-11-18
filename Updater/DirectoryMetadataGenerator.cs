/******************************************************************************
* Filename    = DirectoryMetadataGenerator.cs
*
* Author      = Amithabh A
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = Class to generate metadata of server/client directory
*****************************************************************************/

using System.Security.Cryptography;
using System.Diagnostics;

namespace Updater;

public class DirectoryMetadataGenerator
{

    private List<FileMetadata>? _metadata;

    /// <summary>
    /// Create metadata of directory
    /// </summary>
    /// <param name="directoryPath">Path of the directory</param>
    public DirectoryMetadataGenerator(string? directoryPath)
    {
        directoryPath = directoryPath ??= AppConstants.ToolsDirectory;
        if (!Directory.Exists(directoryPath))
        {
            Trace.WriteLine($"Directory does not exist: {directoryPath}");
            Directory.CreateDirectory(directoryPath);
        }

        _metadata = CreateFileMetadata(directoryPath);
    }


    /// <summary>
    /// Get metadata
    /// </summary>
    /// <returns>List of FileMetadata objects. </returns>
    public List<FileMetadata>? GetMetadata()
    {
        return _metadata;
    }
    /// <summary> Return metadata of the specified directory
    /// </summary>
    /// <param name="directoryPath">Path of directory.</param>
    /// <param name="writeToFile">bool value to write metadata to file.</param>
    /// <returns>List of FileMetadata objects in the directory.</returns>
    public static List<FileMetadata> CreateFileMetadata(string directoryPath)
    {
        directoryPath ??= AppConstants.ToolsDirectory;
        List<FileMetadata> metadata = new List<FileMetadata>();

        foreach (string filePath in Directory.GetFiles(directoryPath))
        {

            metadata.Add(new FileMetadata
            {
                FileName = Path.GetFileName(filePath),
                FileHash = ComputeFileHash(filePath)
            });
        }

        return metadata;
    }

    /// <summary>
    /// Computes SHA-256 hash of file. 
    /// </summary>
    /// <param name="filePath">Path of file</param>
    /// <returns>SHA-256 hash of file</returns>
    public static string ComputeFileHash(string filePath)
    {
        using SHA256 sha256 = SHA256.Create();
        using FileStream stream = File.OpenRead(filePath);
        Byte[] hashBytes = sha256.ComputeHash(stream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
}
