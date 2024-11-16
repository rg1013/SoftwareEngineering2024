// Defines the class "DataPacket" which represents the data packet sent
// from server to client or the other way.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UXModule.ViewModel
{

    // Represents the data packet sent from server to client or the other way.

    public class DataPacket
    {

        // Creates an instance of the DataPacket with empty string values for all

        public DataPacket()
        {
            Id = "";
            Name = "";
            Header = "";
            Data = "";

        }


        // Creates an instance of the DataPacket containing the header field
        // and data field in the packet used for communication between server

        [JsonConstructor]
        public DataPacket(string id, string name, string header, string data, bool isIdle, bool isFull, List<PixelDifference> changedPixels)
        {
            Id = id;
            Name = name;
            Header = header;
            Data = data;
            IsFull = isFull;
            IsIdle = isIdle;
            ChangedPixels = changedPixels;
        }


        // Gets the id field of the packet.

        public string Id { get; set; }

        // Gets the name field of the packet.

        public string Name { get; set; }

        // Gets the header field of the packet.
        // Possible headers from the server: Send, Stop
        // Possible headers from the client: Register, Deregister, Image, Confirmation

        public string Header { get; set; }

        // Gets the data field of the packet.
        // Data corresponding to various headers:

        public string Data { get; set; }
        public bool IsIdle { get; set; }
        public bool IsFull { get; set; }

        public List<PixelDifference> ChangedPixels { get; set; }
    }

    public class PixelDifference
    {
        /// X coordinate of pixel
        public ushort X { get; set; }

        /// Y coordinate of pixel

        public ushort Y { get; set; }

        /// ARGB values of fixel
        public byte Alpha { get; set; }
        public byte Red { get; set; }
        public byte Green { get; set; }
        public byte Blue { get; set; }

        /// constructor to set valuses
        public PixelDifference(ushort x, ushort y, byte alpha, byte red, byte green, byte blue)
        {
            X = x;
            Y = y;
            Red = red;
            Blue = blue;
            Green = green;
            Alpha = alpha;
        }

    }

}