using System.Collections.Generic;
using Rovio.Realtime.Network.Channels.Buffers;

namespace Lem.Networking.Implementation.Channels.Buffers
{
    internal static class BuffersExtensions
    {
        internal static uint BufferState<T>(this NetworkBuffer<T> networkBuffer, uint lastSequence)
            where T : class, IBufferMarker
        {
            var result = 0;

            for (uint index = 0; index < networkBuffer.StateWidthBits; ++index)
            {
                var elem = networkBuffer.At(lastSequence - index);
                if (elem.Initialized)
                {
                    result |= 1 << (int) index;
                }
            }

            return (uint) result;
        }

        internal static bool HasEntries<T>(this NetworkBuffer<T> networkBuffer, uint lastSequence)
            where T : class, IBufferMarker
        {
            for (uint index = 0; index < networkBuffer.StateWidthBits; ++index)
            {
                if (networkBuffer.Has(lastSequence - index))
                {
                    return true;
                }
            }

            return false;
        }

        internal static IEnumerable<T> Entries<T>(this NetworkBuffer<T> networkBuffer, uint lastSequence)
            where T : class, IBufferMarker
        {
            for (var index = networkBuffer.StateWidthBits - 1; index >= 0; --index)
            {
                if (networkBuffer.Has((uint) (lastSequence - index)))
                {
                    yield return networkBuffer.At((uint) (lastSequence - index));
                }
            }
        }
    }
}
