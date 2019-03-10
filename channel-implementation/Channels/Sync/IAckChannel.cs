using System;

namespace Lem.Networking.Implementation.Channels.Sync
{
    internal interface IAckChannel : IChannel
    {
        int PrepareSend(in Span<byte> paddedPacketBuffer);

        ReadOnlySpan<byte> Receive(in ReadOnlySpan<byte> incomingPacketBuffer);

        event Action<int> PacketAcknowledged;
    }
}