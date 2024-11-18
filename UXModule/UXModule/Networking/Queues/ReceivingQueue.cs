using System.Diagnostics;

namespace Networking.Queues;
public class ReceivingQueue
{
    public readonly IQueue Queue = new Queue();

    /// <summary>
    /// Inserts the given packet into the queue
    /// </summary>
    /// <param name="packet">
    /// Packet to be inserted into the queue
    /// </param>
    /// <returns> void </returns>
    public void Enqueue(Packet packet)
    {
        Trace.WriteLine("[Networking] ReceivingQueue.Enqueue() function called.");
        Queue.Enqueue(packet);
    }

    /// <summary>
    /// Removes and returns the front-most packet in the queue
    /// </summary>
    /// <returns>
    /// The front-most element of the queue
    /// </returns>
    public Packet Dequeue()
    {
        Trace.WriteLine("[Networking] ReceivingQueue.Dequeue() function called.");
        return Queue.Dequeue();
    }

    /// <summary>
    /// Returns the front-most packet in the queue
    /// </summary>
    /// <returns>
    /// The front-most element of the queue
    /// </returns>
    public Packet Peek()
    {
        return Queue.Peek();
    }

    /// <summary>
    /// Removes all elements in the queue
    /// </summary>
    /// <returns> void </returns>
    public void Clear()
    {
        Queue.Clear();
    }

    /// <summary>
    /// Returns the size of the queue
    /// </summary>
    /// <returns>
    /// Number of elements in the queue
    /// </returns>
    public int Size()
    {
        return Queue.Size();
    }

    /// <summary>
    /// Returns whether the queue is empty
    /// </summary>
    /// <returns>
    /// 'bool : true' if the queue is empty and 'bool : false' if not
    /// </returns>
    public bool IsEmpty()
    {
        return Queue.IsEmpty();
    }

    /// <summary>
    /// This function is needed to keep listening for packets on the receiving queue
    /// </summary>
    /// <returns>
    /// 'bool : true' if the queue is not empty, else the function keeps waiting for atleast one packet to appear in the queue
    /// and does not return until then
    /// </returns>
    public bool WaitForPacket()
    {
        Trace.WriteLine("[Networking] ReceivingQueue.WaitForPacket() function called.");
        return Queue.WaitForPacket();
    }
}
