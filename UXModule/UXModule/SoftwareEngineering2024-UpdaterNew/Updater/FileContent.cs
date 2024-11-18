/******************************************************************************
* Filename    = FileContent.cs
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

namespace Updater;

[Serializable]
public class FileContent
{
    // Parameterless constructor is required for XML serialization
    public FileContent() { }

    public FileContent(string? fileName, string? serializedContent)
    {
        FileName = fileName;
        SerializedContent = serializedContent;
    }

    [XmlElement("FileName")]
    public string? FileName { get; set; }

    [XmlElement("SerializedContent")]
    public string? SerializedContent { get; set; }

    public override string ToString()
    {
        return $"FileName: {FileName ?? "N/A"}, Content Length: {SerializedContent?.Length ?? 0}";
    }
}
