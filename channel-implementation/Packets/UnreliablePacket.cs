using System;
using Lem.Networking.Implementation.Encryption;
using Lem.Networking.Utilities.Memory;
using Lem.Networking.Utilities.Network;

namespace Lem.Networking.Implementation.Packets
{
    internal ref struct UnreliablePacket
    {
        internal UnreliablePacket(Span<byte> packetBuffer)
        {
            this.PacketBuffer = packetBuffer;
        }

        internal Span<byte> PacketBuffer { get; }

        internal uint ConnectionId
        {
            get => PacketBuffer.RefUint().ToHostOrder();
            set => PacketBuffer.RefUint() = value.ToNetworkOrder();
        }

        internal PacketId PacketId
        {
            get => new PacketId(PacketBuffer.RefUint(SequenceOffset).ToHostOrder());
            set => PacketBuffer.RefUint(SequenceOffset) = value.WireData.ToNetworkOrder();
        }

        internal uint Checksum
        {
            get => PacketBuffer.RefUint(ChecksumOffset).ToHostOrder();
            set => PacketBuffer.RefUint(ChecksumOffset) = value.ToNetworkOrder();
        }

        internal ReadOnlySpan<byte> PacketBody
        {
            get => PacketBuffer.Slice(HeaderSize);
            set => value.CopyTo(PacketBuffer.Slice(HeaderSize));
        }

        internal void Encrypt(IEncryption encryption, ulong initializationVector)
        {
            Checksum = encryption.Checksum(PacketBuffer.Slice(IntegrityOffset));
            encryption.Encrypt(PacketBuffer.Slice(EncryptionOffset), initializationVector);
        }

        internal bool Decrypt(IEncryption encryption, ulong initializationVector)
        {
            encryption.Decrypt(PacketBuffer.Slice(EncryptionOffset), initializationVector);
            return Checksum == encryption.Checksum(PacketBuffer.Slice(IntegrityOffset));
        }

        internal static uint EstimatedSize(in ReadOnlySpan<byte> packetData)
        {
            return EstimatedSize((uint) packetData.Length);
        }

        internal static uint EstimatedSize(uint packetSize)
        {
            return packetSize + HeaderSize;
        }

        internal const int MaxMessageSize = 512;
        internal const int MaxBodySize    = MaxMessageSize - HeaderSize;

        private const int SequenceOffset   = sizeof(uint);
        private const int ChecksumOffset   = sizeof(uint) + SequenceOffset;
        private const int EncryptionOffset = ChecksumOffset;
        private const int IntegrityOffset  = ChecksumOffset + sizeof(uint);

        public const int HeaderSize = IntegrityOffset;
    }
}
