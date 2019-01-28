using System.Runtime.InteropServices;
using Lem.Networking.Utilities.Network;

namespace Lem.Networking.Implementation.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct AckPacketHeader
    {
        private BasePacketHeader baseHeader;
        private ushort           receivedSequence;
        private uint             acksMask;
        
        internal ushort ConnectionId
        {
            get => baseHeader.ConnectionId;
            set => baseHeader.ConnectionId = value;
        }

        internal byte ChannelId
        {
            get => baseHeader.ChannelId;
            set => baseHeader.ChannelId = value;
        }

        internal ushort Sequence
        {
            get => baseHeader.Sequence;
            set => baseHeader.Sequence = value;
        }

        internal uint Checksum
        {
            get => baseHeader.Checksum;
            set => baseHeader.Checksum = value;
        }
        
        internal ushort PayloadLength
        {
            get => baseHeader.PayloadLength;
            set => baseHeader.PayloadLength = value;
        }
        
        internal ChannelAddress ChannelAddress
        {
            get => baseHeader.ChannelAddress;
            set => baseHeader.ChannelAddress = value;
        }

        internal ushort ReceivedSequence
        {
            get => receivedSequence.ToHostOrder();
            set => receivedSequence = value.ToNetworkOrder();
        }

        public uint AcksMask
        {
            get => acksMask.ToHostOrder();
            set => acksMask = value.ToNetworkOrder();
        }

        internal const int ByteSize            = BasePacketHeader.ByteSize + sizeof(ushort) + sizeof(uint);
        internal const int UnencryptedByteSize = BasePacketHeader.UnencryptedByteSize;
    }
}