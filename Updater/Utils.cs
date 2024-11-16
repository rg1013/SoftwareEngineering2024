/******************************************************************************
* Filename    = Utils.cs
*
* Author      = Amithabh A
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = 
*****************************************************************************/

using System.Diagnostics;
using Networking.Serialization;

namespace Updater;

public class Utils
{

    /// <summary>
    /// Reads the content of the specified file.
    /// </summary>
    /// <param name="filePath">Path of file to read. </param>
    /// <returns>Filecontent as string, or null if file dne</returns>
    public static string? ReadBinaryFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.WriteLine("File not found. Please check the path and try again.");
            return null;
        }

        // Read all bytes from the file
        byte[] byteArray = File.ReadAllBytes(filePath);

        // Convert byte array to a base64 string
        return Convert.ToBase64String(byteArray);
    }


    /// <summary>
    /// Write/Overwrite content to file
    /// </summary>
    /// <param name="filePath">Path of file</param>
    /// <param name="content">Content to write.</param>
    public static bool WriteToFileFromBinary(string filePath, string content)
    {
        try
        {
            byte[] data;

            // Check if the content is in base64 format by attempting to decode it
            try
            {
                data = Convert.FromBase64String(content);
            }
            catch (FormatException)
            {
                // If it's not base64, write as a regular string
                File.WriteAllText(filePath, content);
                return true;
            }

            // If decoding to byte array is successful, write as binary
            File.WriteAllBytes(filePath, data);
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"An error occurred while writing to the file: {ex.Message}");
            return false;
        }
    }

    /// <summary> Serializes an object to its string representation.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>A string representation of the serialized object.</returns>
    public static string SerializeObject<T>(T obj)
    {
        try
        {
            ISerializer serializer = new Serializer();
            return serializer.Serialize(obj);
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex.ToString());
            return "";
        }
    }

    /// <summary>
    /// Deserializes a string back to an object of specified type.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize into.</typeparam>
    /// <param name="serializedData">The serialized string data.</param>
    /// <returns>An instance of the specified type.</returns>
    public static T DeserializeObject<T>(string serializedData)
    {
        ISerializer serializer = new Serializer();
        return serializer.Deserialize<T>(serializedData);
    }

    /// <summary>
    /// Generates serialized packet containing metadata of files in a directory.
    /// </summary>
    /// <returns>Serialized packet containing metadata of files in a directory.</returns>
    public static string SerializedMetadataPacket()
    {
        DirectoryMetadataGenerator metadataGenerator = new DirectoryMetadataGenerator();

        if (metadataGenerator == null)
        {
            throw new Exception("Failed to create DirectoryMetadataGenerator");
        }

        List<FileMetadata>? metadata = metadataGenerator.GetMetadata();
        if (metadata == null)
        {
            throw new Exception("Failed to get metadata");
        }

        string serializedMetadata = Utils.SerializeObject(metadata);
        FileContent fileContent = new FileContent("metadata.json", serializedMetadata);
        List<FileContent> fileContents = new List<FileContent> { fileContent };

        DataPacket dataPacket = new DataPacket(DataPacket.PacketType.Metadata, fileContents);
        return SerializeObject(dataPacket);
    }

    /// <summary>
    /// Generates serialized SyncUp packet
    /// </summary>
    /// <returns>Serialized SyncUp packet</returns>
    public static string SerializedSyncUpPacket()
    {
        DataPacket dataPacket = new DataPacket(DataPacket.PacketType.SyncUp, new List<FileContent>());
        return SerializeObject(dataPacket);
    }
}
