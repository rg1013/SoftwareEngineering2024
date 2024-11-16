/******************************************************************************
 * Filename    = Constants.cs
 *
 * Author      = Sai Hemanth Reddy
 *
 * Product     = PlexShare
 * 
 * Project     = FileCloner
 *
 * Description = Defines project-level constants for icon paths, file paths
 *****************************************************************************/

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace FileCloner.Models
{
    /// <summary>
    /// A class in which all project-level constants are declared as properties.
    /// </summary>
    public class Constants
    {
        public static readonly string basePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FileCloner"
        );
        
        // Icon Paths
        public static readonly string loadingIconPath = Path.GetFullPath(Path.Combine("..", "..", "..", "Assets", "Images", "loading.png"));
        public static readonly string fileIconPath = Path.GetFullPath(Path.Combine("..", "..", "..", "Assets", "Images", "file.png"));
        public static readonly string folderIconPath = Path.GetFullPath(Path.Combine("..", "..", "..", "Assets", "Images", "folder.png"));


        // File & Folder Paths
        public static readonly string defaultFolderPath = Path.Combine(basePath, "Temp");
        public static readonly string inputFilePath = Path.Combine(basePath, "input.json");
        public static readonly string outputFilePath = Path.Combine(basePath, "output.json");
        public static readonly string receivedFilesFolderPath = Path.Combine(basePath, "ReceivedFiles");
        public static readonly string senderFilesFolderPath = Path.Combine(basePath, "SenderFiles");

        // Network Service Constants
        public const string success = "success";
        public const string moduleName = "FileCloner";
        public const string request = "request";
        public const string response = "response";
        public const string summary = "summary";
        public const string cloning = "cloning";
        public const string broadcast = "BroadCast";
        public static string IPAddress = GetIP();

        // Size of FileChunk to be sent over network
        public static int FileChunkSize = 1024 * 1024;
        public static int ChunkStartIndex = 1;

        private static string GetIP()
        {
            try
            {
                // Get the IP address of the machine
                IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

                // Iterate through the IP addresses and return the IPv4 address that does not end with "1"
                foreach (IPAddress ipAddress in host.AddressList)
                {
                    if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                    {
                        string address = ipAddress.ToString();
                        if (!address.EndsWith(".1"))
                        {
                            return address;
                        }
                    }
                }
                return "";
            }
            catch (Exception e)
            {
                return "";
            }
            return "";
        }
    }
}
