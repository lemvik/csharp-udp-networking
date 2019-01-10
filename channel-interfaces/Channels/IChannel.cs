using System;
using JetBrains.Annotations;

namespace Lem.Networking.Channels
{
    /// <inheritdoc />
    ///  <summary>
    ///  General channel interface.
    ///
    ///  Channels act as stateful transformers of incoming/outgoing data. Each time a packet need to be sent or
    ///  received the <see cref="Receive"/> or appropriate <c>Send</c> methods should be called on packet data
    ///  to prepare it for transfer/handle it's receiving.
    /// 
    ///  Note that since channels differ in guarantees of delivery, this interface doesn't export <c>Send(...)</c> methods.
    ///  Thread safety is not specified by this interface and depends on the implementation.
    ///  </summary>
    [PublicAPI]
    public interface IChannel : IDisposable
    {
        /// <summary>
        /// Attempt to receive data contained in the given <see cref="packetBuffer"/>. The returned span is 
        /// a reference to message that can be received from channel.
        /// </summary>
        /// <remarks>Note that some channels (reliable in particular) can reorder messages, so the span that is
        /// returned can contain data from some other packet if that one is considered to be ordered before the
        /// given packet.
        /// </remarks>
        /// <param name="packetBuffer">message from remote that this channel should receive</param>
        /// <returns>span corresponding to last received message from this channel (which might or might not
        /// be the one given as <see cref="packetBuffer"/> parameter. If the returned span is empty the channel was
        /// unable to received anything.</returns>
        ref Span<byte> Receive(in Span<byte> packetBuffer);
    }
}