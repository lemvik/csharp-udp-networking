using System;
using JetBrains.Annotations;

namespace Lem.Networking.Channels
{
    /// <inheritdoc />
    /// <summary>
    /// Channel that provides guarantees about delivery and order for packets. However, it has a finite internal
    /// buffer and can reject sends if that buffer overflows.
    /// </summary>
    [PublicAPI]
    public interface IReliableChannel : IChannel
    {
        /// <summary>
        /// Attempts to send packet and returns packetId if send succeeded.
        /// </summary>
        /// <param name="packetBuffer">packet to send</param>
        /// <param name="packetId">ID assigned to the sent packet</param>
        /// <returns>status of sending operation</returns>
        /// <remarks>ID remains valid until <see cref="PacketDelivered"/> is invoked with the ID or channel is reset</remarks>
        bool Send(in ReadOnlySpan<byte> packetBuffer, out int packetId);

        /// <summary>
        /// Fires each time a packet has been delivered. Note that doesn't mean the packet has been processed, just
        /// that the delivery has been successful (it can be so that packet sent before was lost and processing
        /// will await until that packet is delivered also).
        /// </summary>
        event Action<int> PacketDelivered;
    }
}