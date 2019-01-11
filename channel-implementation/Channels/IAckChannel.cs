using System;

namespace Lem.Networking.Implementation.Channels
{
    internal interface IAckChannel : IChannel
    {
        int PrepareSend(in Span<byte> paddedPacketBuffer);

        ReadOnlySpan<byte> Receive(in ReadOnlySpan<byte> incomingPacketBuffer);

        event Action<int> PacketAcknowledged;
    }
}