using System;

namespace Lem.Networking.Utilities.Memory
{
    public interface ISliceOwner<T> : IDisposable where T : unmanaged
    {
        Span<T> Span { get; }
    }
}