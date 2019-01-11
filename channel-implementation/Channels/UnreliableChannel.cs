using System;
using Lem.Networking.Implementation.Packets;

namespace Lem.Networking.Implementation.Channels
{
    internal class UnreliableChannel : IUnreliableChannel
    {
        private readonly ChannelAddress channelAddress;

        internal UnreliableChannel(ChannelAddress channelAddress)
        {
            this.channelAddress = channelAddress;
        }

        public void Dispose()
        {
        }

        public int BufferRequiredByteSize(int desiredPacketByteSize)
        {
            return desiredPacketByteSize + UnreliableHeader.ByteSize;
        }

        public void PrepareSend(in Span<byte> paddedSendPacket)
        {
            if (paddedSendPacket.Length * 8 > Constants.PayloadLengthFieldSize)
            {
                return;
            }

            UnreliableHeader.WriteChannelAddress(paddedSendPacket, channelAddress);
            var payloadLength = (short) (paddedSendPacket.Length - UnreliableHeader.ByteSize);
            UnreliableHeader.WritePayloadLength(paddedSendPacket, payloadLength);
        }

        public ReadOnlySpan<byte> Receive(in ReadOnlySpan<byte> incomingPacketBuffer)
        {
            var address = UnreliableHeader.ReadChannelAddress(incomingPacketBuffer);
            if (address != channelAddress)
            {
                return ReadOnlySpan<byte>.Empty;
            }

            var packetLen = UnreliableHeader.ReadPayloadLength(incomingPacketBuffer);
            if (packetLen > Constants.PayloadLengthFieldSize)
            {
                return ReadOnlySpan<byte>.Empty;
            }

            return incomingPacketBuffer.Slice(UnreliableHeader.ByteSize, packetLen);
        }

        internal static class UnreliableHeader
        {
            internal const int ByteSize = ChannelAddress.ByteSize + Constants.PayloadLengthFieldSize;

            internal static void WriteChannelAddress(in Span<byte> messageBuffer, in ChannelAddress address)
            {
            }

            internal static void WritePayloadLength(in Span<byte> messageBuffer, short payloadLengthInBits)
            {
            }

            internal static ChannelAddress ReadChannelAddress(in ReadOnlySpan<byte> packetBuffer)
            {
                return default;
            }

            internal static short ReadPayloadLength(in ReadOnlySpan<byte> packetBuffer)
            {
                return default;
            }
        }
    }
}