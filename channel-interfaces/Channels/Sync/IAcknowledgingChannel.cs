using System;
using JetBrains.Annotations;
using Lem.Networking.Channels.Sync;

namespace Lem.Networking.Channels
{
    /// <inheritdoc />
    /// <summary>
    /// Channel that makes no guarantees on the order of delivery but attempts to notify about message delivery if
    /// it's possible via <see cref="PacketAcknowledged"/> event.
    /// </summary>
    [PublicAPI]
    public interface IAcknowledgingChannel : IChannel
    {
        /// <summary>
        /// Sends a packet and returns (not-necessary lifetime-unique) identifier of the packet. This identifier
        /// can be used in handler of <see cref="PacketAcknowledged"/> to check if particular packet has been
        /// delivered.
        /// </summary>
        /// <param name="packetBuffer">packet to send to remote</param>
        /// <returns>internal identifier of the message</returns>
        /// <remarks>Note that the acknowledgement itself could be lost, so lack of ack over particular time frame doesn't mean that message has been definitely lost.</remarks>
        /// <remarks>The returned ID can be reused after a while, so tracking currently-flying IDs might be required.</remarks>
        ReadOnlySpan<byte> Send(in ReadOnlySpan<byte> packetBuffer, out int packetId);

        /// <summary>
        /// Fires when packet with particular ID has been acknowledged.
        /// </summary>
        event Action<int> PacketAcknowledged;
    }
}