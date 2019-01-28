using System.Runtime.InteropServices;
using Lem.Networking.Utilities.Network;

namespace Lem.Networking.Implementation.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct BasePacketHeader
    {
        private ushort connectionId;
        private byte   channelId;
        private ushort sequence;
        private uint   checksum;
        private ushort payloadLength;

        internal ushort ConnectionId
        {
            get => connectionId.ToHostOrder();
            set => connectionId = value.ToNetworkOrder();
        }

        internal byte ChannelId
        {
            get => channelId;
            set => channelId = value;
        }

        internal ushort Sequence
        {
            get => sequence.ToHostOrder();
            set => sequence = value.ToNetworkOrder();
        }

        internal uint Checksum
        {
            get => checksum.ToHostOrder();
            set => checksum = value.ToNetworkOrder();
        }

        internal ushort PayloadLength
        {
            get => payloadLength.ToHostOrder();
            set => payloadLength = value.ToNetworkOrder();
        }

        internal ChannelAddress ChannelAddress
        {
            get => new ChannelAddress(ConnectionId, ChannelId);
            set
            {
                ConnectionId = value.ConnectionId;
                ChannelId    = value.ChannelId;
            }
        }
        
        internal const int ByteSize            = sizeof(ushort) + sizeof(byte) + sizeof(ushort) + sizeof(uint) + sizeof(ushort);
        internal const int UnencryptedByteSize = sizeof(ushort) + sizeof(byte) + sizeof(ushort);
    }
}