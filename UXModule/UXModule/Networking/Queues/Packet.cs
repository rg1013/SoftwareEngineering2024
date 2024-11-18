namespace Networking.Queues;
public class Packet
{
    // Serialized data being transmitted
    public string _serializedData;

    // Destination client ID of the packet
    public string _destination;

    // Module which the packet belongs to
    public string _moduleOfPacket;

    // Empty constructor
    public Packet()
    { }

    public Packet(string serializedData, string destination, string moduleOfPacket)
    {
        this._serializedData = serializedData;
        this._destination = destination;
        this._moduleOfPacket = moduleOfPacket;
    }
}
