using System;
using System.Runtime.InteropServices;

namespace Lem.Networking.Implementation.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct ChannelAddress : IEquatable<ChannelAddress>
    {
        public ushort ConnectionId { get; }
        public byte   ChannelId    { get; }

        public ChannelAddress(ushort connectionId, byte channelId)
        {
            this.ConnectionId = connectionId;
            this.ChannelId    = channelId;
        }

        public bool Equals(ChannelAddress other)
        {
            return ConnectionId == other.ConnectionId && ChannelId == other.ChannelId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ChannelAddress other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (ConnectionId * 397) ^ ChannelId.GetHashCode();
            }
        }

        public static bool operator ==(ChannelAddress left, ChannelAddress right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ChannelAddress left, ChannelAddress right)
        {
            return !left.Equals(right);
        }
    }
}