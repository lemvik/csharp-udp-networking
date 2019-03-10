using System;
using System.Runtime.InteropServices;
using Lem.Networking.Utilities.Pools;

namespace Lem.Networking.Utilities.Memory
{
    public static class MemoryExtensions
    {
        public static ref uint RefUint(this Span<byte> span, int offset = 0)
        {
            return ref MemoryMarshal.Cast<byte, uint>(span.Slice(offset, sizeof(uint)))[0];
        }

        public static uint Uint(this ReadOnlySpan<byte> span, int offset = 0)
        {
            return MemoryMarshal.Cast<byte, uint>(span.Slice(offset, sizeof(uint)))[0];
        }

        public static ref ulong RefULong(this Span<byte> span, int offset = 0)
        {
            return ref MemoryMarshal.Cast<byte, ulong>(span.Slice(offset, sizeof(ulong)))[0];
        }

        public static IMemoryResource AllocateCopy(this IMemoryPool   memoryPool,
                                                   ReadOnlySpan<byte> source)
        {
            var memoryBuffer = memoryPool.WithMemory((uint) source.Length);
            source.CopyTo(memoryBuffer.Memory);
            return memoryBuffer;
        }
    }
}
