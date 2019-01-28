using System;
using Lem.Networking.Exceptions;
using Lem.Networking.Implementation.Packets;
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

        public int MaxPayloadSize => Constants.MaximumPayloadSizeBytes - AckPacketHeader.ByteSize;

        public int BufferRequiredByteSize(int desiredPacketByteSize)
        {
            if (desiredPacketByteSize > MaxPayloadSize)
            {
                throw new
                    NetworkConstraintViolationException($"Specified packet size + header exceeds maximum allowed packet size of {MaxPayloadSize}");
            }

            return AckPacketHeader.ByteSize + desiredPacketByteSize;
        }

        public int PrepareSend(in Span<byte> paddedPacketBuffer)
        {
            var lastSentSequence = sentPackets.LastSequence;
            var (lastReceivedSequence, acksMask) = receivedPackets.GenerateAck();
            var sendEntry = sentPackets.Allocate(lastSentSequence);
            sendEntry.Acked = false;

            var header = new AckPacketHeader {
                ChannelAddress   = channelAddress,
                Sequence         = lastSentSequence,
                PayloadLength    = (ushort) (paddedPacketBuffer.Length - BasePacketHeader.ByteSize),
                ReceivedSequence = lastReceivedSequence,
                AcksMask         = (uint) acksMask
            };

            paddedPacketBuffer.Write(header);

            return lastSentSequence;
        }

        public ReadOnlySpan<byte> Receive(in ReadOnlySpan<byte> incomingPacketBuffer)
        {
            incomingPacketBuffer.Read(out AckPacketHeader header);
            var incomingSequence = header.Sequence;
            var _                = receivedPackets.Allocate(incomingSequence);
            var lastRecv         = header.ReceivedSequence;
            var ackMask          = header.AcksMask;

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

            return incomingPacketBuffer.Slice(AckPacketHeader.ByteSize);
        }

        public event Action<int> PacketAcknowledged;

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