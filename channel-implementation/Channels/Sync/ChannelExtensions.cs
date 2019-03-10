namespace Lem.Networking.Implementation.Channels
{
    internal static class ChannelExtensions
    {
        internal static bool IsSequenceGreater(this ushort leftSequence, ushort rightSequence)
        {
            return ((leftSequence > rightSequence) && (leftSequence - rightSequence < ushort.MaxValue / 2))
                   || ((rightSequence > leftSequence) && (rightSequence - leftSequence > ushort.MaxValue / 2));
        }

        internal static bool IsSequenceLess(this ushort leftSequence, ushort rightSequence)
        {
            return IsSequenceGreater(rightSequence, leftSequence);
        }

        internal static bool RestoreRemoteEpoch(this IChannel channel, ushort remoteSequence, out ushort remoteEpoch)
        {
            var channelEpoch    = channel.Epoch;
            var channelSequence = channel.Sequence;

            if (remoteSequence > channelSequence)
            {
                if (remoteSequence - channelSequence < ushort.MaxValue / 2)
                {
                    remoteEpoch = channelEpoch;
                    return true;
                }
                else
                {
                    remoteEpoch = (ushort) (channelEpoch - 1);
                    return true;
                }
            }
            else
            {
                if (channelSequence - remoteSequence < ushort.MaxValue / 2)
                {
                    remoteEpoch = channelEpoch;
                    return true;
                }
                else
                {
                    remoteEpoch = (ushort) (channelEpoch + 1);
                    return true;
                }
            }
        }
    }
}