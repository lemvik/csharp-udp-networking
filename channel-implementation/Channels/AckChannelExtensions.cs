namespace Lem.Networking.Implementation.Channels
{
    internal static class AckChannelExtensions
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
    }
}
