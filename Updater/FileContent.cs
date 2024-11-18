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

[Serializable]
public class FileContent
{
    [XmlElement("FileName")]
    public string? FileName { get; set; }

    [XmlElement("SerializedContent")]
    public string? SerializedContent { get; set; }

    // Parameterless constructor is required for XML serialization
    public FileContent() { }

    public FileContent(string fileName, string serializedContent)
    {
        FileName = fileName;
        SerializedContent = serializedContent;
    }

    public override string ToString()
    {
        return $"FileName: {FileName ?? "N/A"}, Content Length: {SerializedContent?.Length ?? 0}";
    }
}
