namespace Rovio.Realtime.Network.Channels.Buffers
{
    public class BufferMarker : IBufferMarker
    {
        public uint Sequence    { get; private set; }
        public bool Initialized { get; private set; }

        public virtual void Initialize(uint sequence)
        {
            Sequence    = sequence;
            Initialized = true;
        }

        public virtual void Dispose()
        {
            Initialized = false;
        }
    }
}