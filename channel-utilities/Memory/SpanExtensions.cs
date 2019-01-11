using System;
using System.Runtime.InteropServices;

namespace Lem.Networking.Utilities.Memory
{
    public static class SpanExtensions
    {
        public static ref short ShortRef(this Span<byte> span, int byteOffset)
        {
            return ref MemoryMarshal.Cast<byte, short>(span.Slice(byteOffset))[0];
        }
    }
}