using System;
using Lem.Networking.Implementation.Packets;
using Lem.Networking.Utilities.Memory;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;

namespace Lem.Networking.Implementation.Encryption
{
    public class ChaChaEncryption : IEncryption
    {
        private readonly ChaChaEngine encryptor;
        private readonly ChaChaEngine decryptor;
        private readonly byte[]       encryptionIvBuffer;
        private readonly byte[]       decryptionIvBuffer;

        public ChaChaEncryption(ChaChaEngine encryptor, ChaChaEngine decryptor)
        {
            this.encryptor    = encryptor;
            this.decryptor    = decryptor;
            this.encryptionIvBuffer = new byte[sizeof(ulong)];
            this.decryptionIvBuffer = new byte[sizeof(ulong)];
        }

        public bool Encrypt(in Span<byte> outgoingPacket, ulong channelInitVector)
        {
            new Span<byte>(encryptionIvBuffer).RefULong() = channelInitVector;

            // null in parameters means key re-use.
            encryptor.Init(true, new ParametersWithIV(null, encryptionIvBuffer));
            var encryptablePart = outgoingPacket.Slice(BasePacketHeader.UnencryptedByteSize);

            for (var index = 0; index < encryptablePart.Length; ++index)
            {
                encryptablePart[index] = encryptor.ReturnByte(encryptablePart[index]);
            }

            return true;
        }

        public bool Decrypt(in Span<byte> incomingPacket, ulong channelInitVector)
        {
            new Span<byte>(decryptionIvBuffer).RefULong() = channelInitVector;

            // null in parameters means key re-use.
            decryptor.Init(false, new ParametersWithIV(null, decryptionIvBuffer));
            var decryptablePart = incomingPacket.Slice(BasePacketHeader.UnencryptedByteSize);

            for (var index = 0; index < decryptablePart.Length; ++index)
            {
                decryptablePart[index] = decryptor.ReturnByte(decryptablePart[index]);
            }

            return true;
        }

        public uint Checksum(in ReadOnlySpan<byte> packet)
        {
            throw new NotImplementedException();
        }
    }
}
