/******************************************************************************
* Filename    = DataPacket.cs
*
* Author      = Amithabh A
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = Application Data Packet class to encapsulate data for client-server communication
*****************************************************************************/

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

    [XmlElement("PacketType")]
    public PacketType DataPacketType { get; set; }

    [XmlArray("FileContents")]
    [XmlArrayItem("FileContent")]
    public List<FileContent> FileContentList { get; set; } = new List<FileContent>();

    public DataPacket() { }

    // Constructor for multiple files.
    public DataPacket(PacketType packetType, List<FileContent> fileContents)
    {
        DataPacketType = packetType;
        FileContentList = fileContents;
    }
}

