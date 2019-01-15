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

        public static ref ushort UShortRef(this Span<byte> span, int byteOffset)
        {
            return ref MemoryMarshal.Cast<byte, ushort>(span.Slice(byteOffset))[0];
        }

        public static ref int IntRef(this Span<byte> span, int byteOffset)
        {
            return ref MemoryMarshal.Cast<byte, int>(span.Slice(byteOffset))[0];
        }
        
        public static ref byte ByteRef(this Span<byte> span, int byteOffset)
        {
            return ref span.Slice(byteOffset)[0];
        }

        public static short Short(this ReadOnlySpan<byte> span, int byteOffset)
        {
            return MemoryMarshal.Cast<byte, short>(span.Slice(byteOffset))[0];
        }

        public static ushort UShort(this ReadOnlySpan<byte> span, int byteOffset)
        {
            return MemoryMarshal.Cast<byte, ushort>(span.Slice(byteOffset))[0];
        }

        public static int Int(this ReadOnlySpan<byte> span, int byteOffset)
        {
            return MemoryMarshal.Cast<byte, int>(span.Slice(byteOffset))[0];
        }

        public static short Short(this Span<byte> span, int byteOffset)
        {
            return MemoryMarshal.Cast<byte, short>(span.Slice(byteOffset))[0];
        }

        public static ushort UShort(this Span<byte> span, int byteOffset)
        {
            return MemoryMarshal.Cast<byte, ushort>(span.Slice(byteOffset))[0];
        }

        public static int Int(this Span<byte> span, int byteOffset)
        {
            return MemoryMarshal.Cast<byte, int>(span.Slice(byteOffset))[0];
        }
        
        public static byte Byte(this Span<byte> span, int byteOffset)
        {
            return span.Slice(byteOffset)[0];
        }
    }
}