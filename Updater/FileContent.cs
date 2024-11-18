/******************************************************************************
* Filename    = FileContent.cs
*
* Author      = Amithabh A
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = Class to encapsulate file content for client-server communication
*****************************************************************************/

using System.Xml.Serialization;

namespace Updater;

/// <summary>
/// Class to encapsulate file content
/// </summary>
[Serializable]
public class FileContent
{
    [XmlElement("FileName")]
    public string? FileName { get; set; }

    [XmlElement("SerializedContent")]
    public string? SerializedContent { get; set; }

    /// <summary>
    /// Parameterless constructor for serialization
    /// </summary>
    public FileContent() { }

    /// <summary>
    /// Constructor
    /// </summary>
    public FileContent(string fileName, string serializedContent)
    {
        FileName = fileName;
        SerializedContent = serializedContent;
    }

    /// <summary>
    /// Override ToString method
    /// </summary>
    public override string ToString()
    {
        return $"FileName: {FileName ?? "N/A"}, Content Length: {SerializedContent?.Length ?? 0}";
    }
}
