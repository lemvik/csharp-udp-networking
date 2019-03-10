using System;
using Lem.Networking.Implementation.Packets;

namespace Lem.Networking.Implementation.Channels
{
    internal struct ChannelState
    {
        private const int  SeqBits      = 21;
        private const int  AckBits      = 8;
        private const uint SeqMask      = (1u << SeqBits) - 1u; // Mask used to extract ticks from timestamp.
        private const uint EpochMask    = ~SeqMask; // Mask used to extract epoch from timestamp.
        private const byte MaxAcks      = byte.MaxValue; // Max acks to send in single frame/timestamp.
        public const  uint MaxTicksDiff = 16384u;

        public ChannelState(uint ticks, uint epoch, uint acks)
        {
            Ticks = 0;
            Ticks = ticks;
            Epoch = epoch;
            Acks  = acks;
            if (Acks >= MaxAcks)
                throw new Exception(message: "Attempted to create channel state with acks exceeding MAX_ACKS.");

            if (SeqMask < Ticks)
                throw new Exception(message: "Attempted to create channel state with Ticks exceeding SEQ_MASK");
        }

        public uint Ticks { get; private set; }
        public uint Epoch { get; private set; }
        public uint Acks  { get; private set; }

        public uint Timestamp => (Epoch << SeqBits) | Ticks;
        public uint Sequence  => (Timestamp << AckBits) | Acks;

        public void Reset()
        {
            Ticks = 0;
            Epoch = 0;
            Acks  = 0;
        }

        public bool AddAck()
        {
            if (Acks == MaxAcks) return false;

            ++Acks;
            return true;
        }

        public void ResetAcks()
        {
            Acks = 0;
        }

        public void Tick()
        {
            if (Ticks == SeqMask)
            {
                Ticks = 0;
                ++Epoch;
            }
            else
            {
                ++Ticks;
            }
        }

        public void AdjustTo(uint timestamp)
        {
            Ticks = timestamp & SeqMask;
            Epoch = (timestamp & EpochMask) >> SeqBits;
            Acks  = 0;
        }

        public void AdjustTo(ChannelState remoteState)
        {
            Ticks = remoteState.Ticks;
            Epoch = remoteState.Epoch;
            Acks  = 0;
        }

        public void AdjustTo(PacketId remoteSequence)
        {
            if (RestoreRemoteState(remoteSequence, out var newState))
            {
                AdjustTo(newState);
            }
            else
            {
                throw new Exception("Unable to adjust to invalid remote state.");
            }
        }

        public override string ToString()
        {
            return $"ChannelState[Ticks={Ticks},Acks={Acks},Epoch={Epoch}]";
        }

        public bool RestoreRemoteState(PacketId remotePacket, out ChannelState remoteState)
        {
            const uint upperInterval = SeqMask - MaxTicksDiff;
            var        localTicks    = Ticks;
            var        remoteTicks   = remotePacket.Number;

            if (Math.Abs((int) localTicks - (int) remoteTicks) < MaxTicksDiff)
            {
                remoteState = new ChannelState(remotePacket.Number, Epoch, remotePacket.AckCounter);
                return true;
            }

            if (localTicks > upperInterval && remoteTicks < MaxTicksDiff)
            {
                remoteState = new ChannelState(remotePacket.Number, Epoch + 1, remotePacket.AckCounter);
                return true;
            }

            if (remoteTicks > upperInterval && localTicks < MaxTicksDiff)
            {
                remoteState = new ChannelState(remotePacket.Number, Epoch - 1, remotePacket.AckCounter);
                return true;
            }

            remoteState = default;
            return false;
        }

        public ulong EncryptionNonce(PacketId remotePacket)
        {
            return ((ulong) Epoch << 32) | remotePacket.WireData;
        }
    }
}
