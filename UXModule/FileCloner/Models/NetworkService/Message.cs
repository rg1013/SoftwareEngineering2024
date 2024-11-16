/******************************************************************************
 * Filename    = Message.cs
 *
 * Author      = Sai Hemanth Reddy
 * 
 * Project     = FileCloner
 *
 * Description = Represents a message used for network communication in the 
 *               FileCloner application. The message includes metadata like 
 *               subject, sender, recipient, and additional data used to handle
 *               file cloning requests and responses.
 *****************************************************************************/

using System;

namespace FileCloner.Models.NetworkService
{
    /// <summary>
    /// Represents a message object for network communication, including metadata 
    /// and file content for handling file cloning requests and responses.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// The subject or type of the message, e.g., request, response, summary, cloning.
        /// </summary>
        public string Subject { get; set; } = "";

        /// <summary>
        /// Unique identifier for each request, used to correlate requests and responses.
        /// </summary>
        public int RequestID { get; set; } = -1;

        /// <summary>
        /// IP address or identifier of the sender.
        /// </summary>
        public string From { get; set; } = "";

        /// <summary>
        /// IP address or identifier of the recipient. 
        /// If broadcasting, this may be a wildcard.
        /// </summary>
        public string To { get; set; } = "";

        /// <summary>
        /// Additional metadata about the message, such as the requester path in cloning operations.
        /// </summary>
        public string MetaData { get; set; } = "";

        /// <summary>
        /// The body of the message, containing file content or summary data as needed.
        /// </summary>
        public string Body { get; set; } = "";
    }
}
