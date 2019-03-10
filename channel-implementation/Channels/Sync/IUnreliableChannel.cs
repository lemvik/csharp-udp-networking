using System;

namespace Lem.Networking.Implementation.Channels
{
    internal interface IUnreliableChannel : IChannel
    {
        void PrepareSend(in Span<byte> paddedSendPacket);

        ReadOnlySpan<byte> Receive(in ReadOnlySpan<byte> incomingPacketBuffer, out ushort sequence);
    }
}