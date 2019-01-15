using System;

namespace Lem.Networking.Implementation.Packets
{
    internal struct ChannelAddress : IEquatable<ChannelAddress>
    {
        public int  ConnectionId { get; }
        public byte ChannelId    { get; }

        public const int ByteSize = sizeof(int) + sizeof(byte);

        public ChannelAddress(int connectionId, byte channelId)
        {
            ConnectionId = connectionId;
            ChannelId    = channelId;
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