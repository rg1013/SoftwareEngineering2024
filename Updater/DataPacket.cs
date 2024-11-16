/******************************************************************************
* Filename    = DataPacket.cs
*
* Author      = Amithabh A
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = 
*****************************************************************************/

using System.Text;
using System.Xml.Serialization;

namespace Updater;

[Serializable]
public class DataPacket
{
    public enum PacketType
    {
        SyncUp,        // No files
        InvalidSync,   // No files
        Metadata,      // single file
        Differences,   // multiple files
        ClientFiles,   // multiple files
        Broadcast      // multiple files
    }

    public DataPacket() { }

    // Constructor for multiple files.
    public DataPacket(PacketType packetType, List<FileContent> fileContents)
    {
        DataPacketType = packetType;
        FileContentList = fileContents ?? new List<FileContent>();
    }

    [XmlElement("PacketType")]
    public PacketType DataPacketType { get; set; }

    [XmlArray("FileContents")]
    [XmlArrayItem("FileContent")]
    public List<FileContent> FileContentList { get; set; } = new List<FileContent>();

    public override string ToString()
    {
        StringBuilder formattedOutput = new StringBuilder();
        formattedOutput.AppendLine($"Packet Type: {DataPacketType}");

        if (FileContentList.Count > 0)
        {
            formattedOutput.AppendLine("Multiple Files:");
            foreach (FileContent file in FileContentList)
            {
                formattedOutput.AppendLine(file.ToString()); // Assuming FileContent has a ToString method
            }
        }
        else
        {
            formattedOutput.AppendLine("No files in the packet.");
        }

        return formattedOutput.ToString();
    }
}
