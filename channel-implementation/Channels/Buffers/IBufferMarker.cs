using System;

namespace Rovio.Realtime.Network.Channels.Buffers
{
    public interface IBufferMarker : IDisposable
    {
        uint Sequence    { get; }
        bool Initialized { get; }
        void Initialize(uint sequence);
    }
}