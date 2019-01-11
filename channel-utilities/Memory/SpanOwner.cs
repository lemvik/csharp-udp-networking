using System.Buffers;

namespace Lem.Networking.Utilities.Memory
{
    public ref struct SpanOwner
    {
        private IMemoryOwner<byte> memoryOwner;
        private int                offset;
        private int                size;

        public SpanOwner(IMemoryOwner<byte> memoryOwner, int offset, int size)
        {
            this.memoryOwner = memoryOwner;
            this.offset      = offset;
            this.size        = size;
        }

        public void Dispose()
        {
            
        }
    }
}