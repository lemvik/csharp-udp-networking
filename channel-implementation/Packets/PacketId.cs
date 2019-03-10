using System;
using Lem.Networking.Utilities.Memory;

namespace Lem.Networking.Implementation.Packets
{
    internal struct PacketId
    {
        // 32 bits of header first uint are split this way.
        private const int SequenceBits = 21;
        private const int SemanticBits = 3;
        private const int AcksBits     = 8;

        // Sequence mask: 0000 0000 0001 1111 1111 1111 1111 1111
        private const uint SequenceMask = (1u << SequenceBits) - 1u;

        // Semantic mask: 0000 0000 0000 0000 0000 0000 0000 0111
        private const uint SemanticMask = (1u << SemanticBits) - 1u;

        // Ack mask:      0000 0000 0000 0000 0000 0000 1111 1111
        private const uint AcksMask = (1u << AcksBits) - 1u;

        // Shifted sem:   0000 0000 1110 0000 0000 0000 0000 0000
        private const int SemShift = SequenceBits;

        // Shifted ack:   1111 1111 0000 0000 0000 0000 0000 0000
        private const int AckShift = SequenceBits + SemanticBits;

        public uint WireData;

        public uint Number
        {
            get => WireData & SequenceMask;

            private set
            {
                WireData &= ~SequenceMask;
                WireData |= value & SequenceMask;
            }
        }

        public ushort Id
        {
            get => (ushort) ((WireData >> SemShift) & SemanticMask);

            private set
            {
                WireData &= ~(SemanticMask << SemShift);
                WireData |= (value & SemanticMask) << SemShift;
            }
        }

        public uint AckCounter
        {
            get => (WireData >> AckShift) & AcksMask;

            private set
            {
                WireData &= ~(AcksMask << AckShift);
                WireData |= (value & AcksMask) << AckShift;
            }
        }

        public PacketId(ushort id, uint number, uint ackCounter)
        {
            WireData   = 0;
            Id         = id;
            Number     = number;
            AckCounter = ackCounter;
        }

        public PacketId(uint wireData)
        {
            WireData = wireData;
        }

        public override string ToString()
        {
            return $"Sequence[ID={Id},Number={Number},Acks={AckCounter},Wire={WireData}]";
        }

        internal static PacketId Peek(ReadOnlySpan<byte> packet)
        {
            return new PacketId(packet.Uint());
        }
    }
}
