using System;
using Lem.Networking.Implementation.Packets;

namespace Lem.Networking.Implementation.Channels
{
    internal class UnreliableChannel : IUnreliableChannel
    {
        private readonly ChannelAddress channelAddress;
        private          uint sequence;

        internal UnreliableChannel(ChannelAddress channelAddress)
        {
            this.channelAddress = channelAddress;
        }

        public void Dispose()
        {
        }


        public ushort Epoch => (ushort) (sequence >> (sizeof(ushort) * 8));
        public ushort Sequence => (ushort) sequence;
        public int    MaxPayloadSize  => Constants.MaximumPayloadSizeBytes - BasePacketHeader.ByteSize;

        public int BufferRequiredByteSize(int desiredPacketByteSize)
        {
            return desiredPacketByteSize + BasePacketHeader.ByteSize;
        }

        public void PrepareSend(in Span<byte> paddedSendPacket)
        {
            if (paddedSendPacket.Length * 8 > Constants.PayloadLengthFieldSize)
            {
                return;
            }

            paddedSendPacket.Write(new BasePacketHeader {
                ChannelAddress = channelAddress,
                Sequence       = (ushort) ++sequence,
                PayloadLength  = (ushort) (paddedSendPacket.Length - BasePacketHeader.ByteSize)
            });
        }

        public ReadOnlySpan<byte> Receive(in ReadOnlySpan<byte> incomingPacketBuffer, out ushort packetSequence)
        {
            if (!incomingPacketBuffer.Read(out BasePacketHeader header))
            {
                packetSequence = default;
                return ReadOnlySpan<byte>.Empty;
            }

            if (header.ChannelAddress != channelAddress)
            {
                packetSequence = default;
                return ReadOnlySpan<byte>.Empty;
            }

            packetSequence = header.Sequence;
            var packetLength = header.PayloadLength;
            return incomingPacketBuffer.Slice(BasePacketHeader.ByteSize, packetLength);
        }
    }
}