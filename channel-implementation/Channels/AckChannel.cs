using System;
using Lem.Networking.Exceptions;
using Lem.Networking.Implementation.Packets;
using Lem.Networking.Utilities.Memory;
using Lem.Networking.Utilities.Resources;

namespace Lem.Networking.Implementation.Channels
{
    internal class AckChannel : IAckChannel
    {
        private readonly ChannelAddress                 channelAddress;
        private readonly SequenceBuffer<SentPacket>     sentPackets;
        private readonly SequenceBuffer<ReceivedPacket> receivedPackets;

        public AckChannel(ChannelAddress channelAddress, int buffersSize)
        {
            this.channelAddress = channelAddress;
            sentPackets         = new SequenceBuffer<SentPacket>(buffersSize);
            receivedPackets     = new SequenceBuffer<ReceivedPacket>(buffersSize);
        }

        public void Dispose()
        {
        }

        public int MaxPayloadSize => Constants.MaximumPayloadSizeBytes - AckChannelHeader.ByteSize;

        public int BufferRequiredByteSize(int desiredPacketByteSize)
        {
            if (desiredPacketByteSize > MaxPayloadSize)
            {
                throw new
                    NetworkConstraintViolationException($"Specified packet size + header exceeds maximum allowed packet size of {MaxPayloadSize}");
            }

            return AckChannelHeader.ByteSize + desiredPacketByteSize;
        }

        public int PrepareSend(in Span<byte> paddedPacketBuffer)
        {
            var lastSentSequence = sentPackets.LastSequence;
            var (lastReceivedSequence, acksMask) = receivedPackets.GenerateAck();
            var sendEntry = sentPackets.Allocate(lastSentSequence);
            sendEntry.Acked = false;

            var _ = new AckChannelHeader(paddedPacketBuffer) {
                Address       = channelAddress,
                Sequence      = lastSentSequence,
                RecvSequence  = lastReceivedSequence,
                AckMask       = acksMask,
                PayloadLength = (ushort) (paddedPacketBuffer.Length - AckChannelHeader.ByteSize)
            };

            return lastSentSequence;
        }

        public ReadOnlySpan<byte> Receive(in ReadOnlySpan<byte> incomingPacketBuffer)
        {
            var header           = new IncomingAckChannelHeader(incomingPacketBuffer);
            var incomingSequence = header.Sequence;
            var _                = receivedPackets.Allocate(incomingSequence);
            var lastRecv         = header.RecvSequence;
            var ackMask          = header.AckMask;

            for (ushort index = 0; index < sizeof(int) * 8; ++index)
            {
                if ((ackMask & 1) == 1)
                {
                    var sequence = (ushort) (lastRecv - index);
                    var metadata = sentPackets.Element(sequence);
                    if (metadata != null && !metadata.Acked)
                    {
                        metadata.Acked = true;
                        PacketAcknowledged?.Invoke(sequence);
                    }
                }

                ackMask >>= 1;
            }

            return incomingPacketBuffer.Slice(AckChannelHeader.PayloadOffset);
        }

        public event Action<int> PacketAcknowledged;

        internal ref struct AckChannelHeader
        {
            internal const int AddressOffset       = 0;
            internal const int PayloadLengthOffset = AddressOffset + ChannelAddress.ByteSize;
            internal const int SequenceOffset      = PayloadLengthOffset + Constants.PayloadLengthFieldSize;
            internal const int RecvSequenceOffset  = SequenceOffset + sizeof(ushort);
            internal const int AckMaskOffset       = RecvSequenceOffset + sizeof(ushort);
            internal const int PayloadOffset       = AckMaskOffset + sizeof(int);

            internal const int ByteSize = PayloadOffset;

            private Span<byte> packetBuffer;

            public AckChannelHeader(Span<byte> packetBuffer)
            {
                this.packetBuffer = packetBuffer;
                Address           = default;
            }

            internal ChannelAddress Address
            {
                get => new ChannelAddress(packetBuffer.Int(0), packetBuffer.Byte(sizeof(int)));

                set
                {
                    packetBuffer.IntRef(0)            = value.ConnectionId;
                    packetBuffer.ByteRef(sizeof(int)) = value.ChannelId;
                }
            }

            internal ref ushort PayloadLength => ref packetBuffer.UShortRef(PayloadLengthOffset);
            internal ref ushort Sequence      => ref packetBuffer.UShortRef(SequenceOffset);
            internal ref ushort RecvSequence  => ref packetBuffer.UShortRef(RecvSequenceOffset);
            internal ref int    AckMask       => ref packetBuffer.IntRef(AckMaskOffset);
        }

        internal ref struct IncomingAckChannelHeader
        {
            private ReadOnlySpan<byte> packetBuffer;

            public IncomingAckChannelHeader(ReadOnlySpan<byte> packetBuffer)
            {
                this.packetBuffer = packetBuffer;
                Address           = default;
            }

            internal ChannelAddress Address { get; }

            internal short  PayloadLength => packetBuffer.Short(AckChannelHeader.PayloadLengthOffset);
            internal ushort Sequence      => packetBuffer.UShort(AckChannelHeader.SequenceOffset);
            internal ushort RecvSequence  => packetBuffer.UShort(AckChannelHeader.RecvSequenceOffset);
            internal int    AckMask       => packetBuffer.Int(AckChannelHeader.AckMaskOffset);
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
            }
        }
    }
}