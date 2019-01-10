using System;
using JetBrains.Annotations;

namespace Lem.Networking.Channels
{
    /// <inheritdoc />
    /// <summary>
    /// Channel that makes no guarantees about order or completion of delivery.
    /// </summary>
    /// <remarks>No attempts made to make sure message has been even sent.</remarks>
    [PublicAPI]
    public interface IUnreliableChannel : IChannel
    {
        /// <summary>
        /// Attempts to send packet to the remote. 
        /// </summary>
        /// <param name="packetBuffer">packet to send to the remote</param>
        /// <remarks>if packet exceeds <see cref="Constants.MaximumPacketSize"/> bytes exceeding bytes won't be sent.</remarks>
        /// <returns>buffer that can be sent over the wire. If it's empty then channel is unable to prepare the
        /// send operation.</returns>
        ReadOnlySpan<byte> Send(in ReadOnlySpan<byte> packetBuffer);
    }
}