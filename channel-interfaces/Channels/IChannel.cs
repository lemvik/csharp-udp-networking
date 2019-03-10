using System;
using JetBrains.Annotations;

namespace Lem.Networking.Channels
{
    /// <inheritdoc />
    ///  <summary>
    ///  General synchronous channel interface.
    ///
    ///  Channels act as stateful transformers of incoming/outgoing data. Each time a packet need to be sent or
    ///  received the <c>Receive</c> or appropriate <c>Send</c> methods should be called on packet data
    ///  to prepare it for transfer/handle it's receiving.
    /// 
    ///  Note that since channels differ in guarantees of delivery, this interface doesn't export <c>Send(...)</c> methods.
    ///  Thread safety is not specified by this interface and depends on the implementation.
    ///  </summary>
    [PublicAPI]
    public interface IChannel : IDisposable
    {
    }
}