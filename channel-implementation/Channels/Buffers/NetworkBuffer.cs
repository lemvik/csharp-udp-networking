using System;
using System.Diagnostics;

namespace Rovio.Realtime.Network.Channels.Buffers
{
    internal class NetworkBuffer<TValue> : IDisposable where TValue : class, IBufferMarker
    {
        private readonly uint     bufferCapacity;
        private readonly TValue[] storage;

        private uint lastRetainMarker;

        public NetworkBuffer(Func<TValue> factory, uint bufferCapacity)
        {
            Debug.Assert(bufferCapacity >= sizeof(uint) * 8, "bufferCapacity.IsPowerOfTwo()");
            this.bufferCapacity = bufferCapacity;
            this.storage        = new TValue[bufferCapacity];

            for (var index = 0; index < bufferCapacity; ++index)
            {
                storage[index] = factory();
            }
        }

        public int StateWidthBits => 32;

        public void Dispose()
        {
            Reset();
        }

        public void Reset()
        {
            foreach (var value in storage)
            {
                value.Dispose();
            }
        }

        public TValue Set(uint index)
        {
            var element = storage[index % bufferCapacity];
            if (element.Initialized)
            {
                element.Dispose();
            }

            element.Initialize(index);
            return element;
        }

        public void Unset(uint index)
        {
            storage[index % bufferCapacity].Dispose();
        }

        public bool Has(uint index)
        {
            return storage[index % bufferCapacity].Initialized;
        }

        public TValue At(uint index)
        {
            return storage[index % bufferCapacity];
        }

        public void Retain(uint lastSequence, int howMany = sizeof(uint) * 8)
        {
            var lastToDrop = (lastSequence - howMany) % bufferCapacity;
            var index      = lastRetainMarker;
            while (index != lastToDrop)
            {
                var element = storage[index];
                element.Dispose();
                index = (index + 1) % bufferCapacity;
            }

            lastRetainMarker = (uint) lastToDrop;
        }
    }
}
