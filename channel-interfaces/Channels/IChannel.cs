using System;
using JetBrains.Annotations;

namespace Lem.Networking.Channels
{
    /// <inheritdoc />
    ///  <summary>
    ///  <para>General channel interface.</para>
    ///  <para>Channel should be updated via call to <see cref="IChannel.Update()"/> on a regular basis to ensure that implementations can perform resending/events
    ///  invocations.</para>
    ///  <para><see cref="IChannel.Receive(Span{byte},int)"/> method can be called to receive incoming messages
    ///  from the remote end. It should be called in a loop until it returns false to flush all pending messages in
    ///  the channel.</para>
    ///  <para>Note that since channels differ in guarantees of delivery, this interface doesn't export <c>Send(...)</c> methods.</para>
    ///  <para>Thread safety is not specified by this interface and depends on the implementation.</para>
    ///  </summary>
    [PublicAPI]
    public interface IChannel : IDisposable
    {
        /// <summary>
        /// Attempts to receive any messages from remote.
        /// </summary>
        /// <param name="packetBuffer">buffer to receive packet into, message gets truncated if there is not enough space in buffer.</param>
        /// <param name="receivedBytes">bytes written into message buffer.</param>
        /// <returns><c>true</c> if message was received and there might be more, <c>false</c> if not.</returns>
        bool Receive(in Span<byte> packetBuffer, out int receivedBytes);

        /// <summary>
        /// Updates the internal state of the channel, possibly populating buffers/re-sending packets.
        /// </summary>
        void Update();
    }
}