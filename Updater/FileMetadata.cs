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

public class FileMetadata
{
    public string? FileName { get; set; }
    public string? FileHash { get; set; }

    public override string ToString()
    {
        return $"FileName: {FileName ?? "N/A"}, FileHash: {FileHash ?? "N/A"}";
    }
}

