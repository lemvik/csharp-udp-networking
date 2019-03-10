using System;
using System.Buffers;
using System.Diagnostics;
using Microsoft.Extensions.ObjectPool;

namespace Lem.Networking.Utilities.Pools
{
    public class MemoryPool : IMemoryPool
    {
        private readonly DefaultObjectPool<PooledMemoryResource> resources;

        public MemoryPool()
        {
            resources = new DefaultObjectPool<PooledMemoryResource>(new PooledMemoryResourcePolicy(this));
        }

        private static IMemoryOwner<byte> GetMemory(uint size)
        {
            return MemoryPool<byte>.Shared.Rent((int) size);
        }

        public IMemoryResource WithMemory(uint size)
        {
            var resource = resources.Get();
            resource.Initialize(size);
            return resource;
        }

        private class PooledMemoryResource : IMemoryResource
        {
            private readonly ObjectPool<PooledMemoryResource> parentPool;

            private IMemoryOwner<byte> memoryOwner;
            private bool               disposed;
            private int                size;

            public PooledMemoryResource(ObjectPool<PooledMemoryResource> parentPool)
            {
                this.parentPool = parentPool;
            }

            public void Initialize(uint requiredSize)
            {
                this.size   = (int) requiredSize;
                disposed    = false;
                memoryOwner = GetMemory(requiredSize);
            }

            public void Dispose()
            {
                Debug.Assert(!disposed, $"Double disposal of a memory block of [size={size}]");
                disposed = true;
                memoryOwner.Dispose();
                parentPool.Return(this);
            }

            public Span<byte> Memory => memoryOwner.Memory.Span.Slice(0, size);
        }

        private class PooledMemoryResourcePolicy : PooledObjectPolicy<PooledMemoryResource>
        {
            private readonly MemoryPool ownerPool;

            public PooledMemoryResourcePolicy(MemoryPool ownerPool)
            {
                this.ownerPool = ownerPool;
            }

            public override PooledMemoryResource Create()
            {
                return new PooledMemoryResource(ownerPool.resources);
            }

            public override bool Return(PooledMemoryResource resource)
            {
                return true;
            }
        }
    }
}
