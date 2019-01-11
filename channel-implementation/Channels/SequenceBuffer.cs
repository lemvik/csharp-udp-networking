using System.Linq;
using Lem.Networking.Utilities.Resources;

namespace Lem.Networking.Implementation.Channels
{
    public class SequenceBuffer<TElement> : IResettable where TElement : class, IResettable, new()
    {
        private readonly int        bufferSize;
        private readonly int[]      sequence;
        private readonly TElement[] elements;
        private          int        lastSequence;

        public SequenceBuffer(int size)
        {
            bufferSize   = size;
            sequence     = new int[bufferSize];
            elements     = Enumerable.Range(0, bufferSize).Select(_ => new TElement()).ToArray();
            lastSequence = 0;
        }

        public TElement Element(int sequenceNumber)
        {
            var index = sequenceNumber % bufferSize;
            if (sequence[index] == sequenceNumber)
            {
                return elements[index];
            }

            return default;
        }

        public TElement Allocate(int sequenceNumber)
        {
            var index = sequenceNumber % bufferSize;
            sequence[index] = sequenceNumber;
            elements[index].Reset();
            return elements[index];
        }

        public int LastSequence => lastSequence + 1;

        public void Reset()
        {
            for (var index = 0; index < bufferSize; ++index)
            {
                sequence[index] = default;
                elements[index].Reset();
            }
            lastSequence = 0;
        }
    }
}