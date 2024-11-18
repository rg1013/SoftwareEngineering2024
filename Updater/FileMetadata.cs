/******************************************************************************
* Filename    = FileMetadata.cs
*
* Author      = Amithabh A
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = Class to encapsulate file metadata for client-server communication
*****************************************************************************/

namespace Updater;

/// <summary>
/// Class to encapsulate file metadata
/// </summary>
public class FileMetadata
{
    public string? FileName { get; set; }
    public string? FileHash { get; set; }

    /// <summary>
    /// override ToString method
    /// </summary>
    public override string ToString()
    {
        return $"FileName: {FileName ?? "N/A"}, FileHash: {FileHash ?? "N/A"}";
    }
}

