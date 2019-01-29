using System;

namespace Lem.Networking.Implementation.Encryption
{
    public interface IEncryption
    {
        bool Encrypt(in Span<byte> outgoingPacket, ulong channelInitVector);
        bool Decrypt(in Span<byte> incomingPacket, ulong channelInitVector);

        uint Checksum(in ReadOnlySpan<byte> packet);
    }
}