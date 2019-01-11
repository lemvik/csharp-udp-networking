using System;
using Lem.Networking.Implementation.Packets;
using Lem.Networking.Utilities.Memory;
using Lem.Networking.Utilities.Resources;

namespace Lem.Networking.Implementation.Channels
{
    internal class AckChannel : IAckChannel
    {
        private readonly SequenceBuffer<SentPacket>     sentPackets;
        private readonly SequenceBuffer<ReceivedPacket> receivedPackets;

        public void Dispose()
        {
        }

        public int BufferRequiredByteSize(int desiredPacketByteSize)
        {
            return AckChannelHeader.ByteSize + desiredPacketByteSize;
        }

        public int PrepareSend(in Span<byte> paddedPacketBuffer)
        {
            throw new NotImplementedException();
        }

        public ReadOnlySpan<byte> Receive(in ReadOnlySpan<byte> incomingPacketBuffer)
        {
            throw new NotImplementedException();
        }

        public event Action<int> PacketAcknowledged;

        internal ref struct AckChannelHeader
        {
            internal const int ByteSize = ChannelAddress.ByteSize + Constants.PayloadLengthFieldSize + sizeof(int);

            private Span<byte> packetBuffer;

            public AckChannelHeader(Span<byte> packetBuffer)
            {
                this.packetBuffer = packetBuffer;
                Address           = default;
            }

            internal ChannelAddress Address { get; set; }

            internal ref short PayloadLength => ref packetBuffer.ShortRef(ChannelAddress.ByteSize);
        }

        internal class SentPacket : IResettable
        {
            public bool Acked { get; set; }

            public void Reset()
            {
                Acked = false;
            }
        }

        internal class ReceivedPacket : IResettable
        {
            public void Reset()
            {
                throw new NotImplementedException();
            }
        }
    }
}