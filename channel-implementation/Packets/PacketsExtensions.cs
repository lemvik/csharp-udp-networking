using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Lem.Networking.Implementation.Packets
{
    internal static class PacketsExtensions
    {
        internal static bool Write(this Span<byte> targetSpan, BasePacketHeader header)
        {
            Debug.Assert(targetSpan.Length >= BasePacketHeader.ByteSize);
            return MemoryMarshal.TryWrite(targetSpan, ref header);
        }

        internal static bool Read(this ReadOnlySpan<byte> sourceSpan, out BasePacketHeader header)
        {
            Debug.Assert(sourceSpan.Length >= BasePacketHeader.ByteSize);
            return MemoryMarshal.TryRead(sourceSpan, out header);
        }
        
        internal static bool Write(this Span<byte> targetSpan, AckPacketHeader header)
        {
            Debug.Assert(targetSpan.Length >= AckPacketHeader.ByteSize);
            return MemoryMarshal.TryWrite(targetSpan, ref header);
        }

        internal static bool Read(this ReadOnlySpan<byte> sourceSpan, out AckPacketHeader header)
        {
            Debug.Assert(sourceSpan.Length >= AckPacketHeader.ByteSize);
            return MemoryMarshal.TryRead(sourceSpan, out header);
        }
    }
}