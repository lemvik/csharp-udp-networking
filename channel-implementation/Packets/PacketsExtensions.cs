using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Lem.Networking.Implementation.Packets
{
    internal static class PacketsExtensions
    {
        internal static bool Write(this Span<byte> targetSpan, BasePacketHeader header)
        {
            return MemoryMarshal.TryWrite(targetSpan, ref header);
        }

        internal static bool Read(this ReadOnlySpan<byte> sourceSpan, out BasePacketHeader header)
        {
            return MemoryMarshal.TryRead(sourceSpan, out header);
        }
        
        internal static bool Read(this Span<byte> sourceSpan, out BasePacketHeader header)
        {
            return MemoryMarshal.TryRead(sourceSpan, out header);
        }
        
        internal static bool Write(this Span<byte> targetSpan, AckPacketHeader header)
        {
            return MemoryMarshal.TryWrite(targetSpan, ref header);
        }

        internal static bool Read(this ReadOnlySpan<byte> sourceSpan, out AckPacketHeader header)
        {
            return MemoryMarshal.TryRead(sourceSpan, out header);
        }
        
        internal static bool Read(this Span<byte> sourceSpan, out AckPacketHeader header)
        {
            return MemoryMarshal.TryRead(sourceSpan, out header);
        }
    }
}