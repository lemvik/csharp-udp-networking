using System;

namespace Lem.Networking.Utilities.Pools
{
    public interface IMemoryResource : IDisposable
    {
        Span<byte> Memory { get; }
    }

    public interface IMemoryPool
    {
        IMemoryResource WithMemory(uint size);
    }
}