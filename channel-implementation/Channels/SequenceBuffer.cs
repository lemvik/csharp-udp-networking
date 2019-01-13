using System.Linq;
using Lem.Networking.Utilities.Resources;

namespace Lem.Networking.Implementation.Channels
{
    public class SequenceBuffer<TElement> : IResettable where TElement : class, IResettable, new()
    {
        private readonly int        bufferSize;
        private readonly ushort[]   sequence;
        private readonly TElement[] elements;
        private          ushort     lastSequence;

        public SequenceBuffer(int size)
        {
            bufferSize   = size;
            sequence     = new ushort[bufferSize];
            elements     = Enumerable.Range(0, bufferSize).Select(_ => new TElement()).ToArray();
            lastSequence = 0;
        }

        public TElement Element(ushort sequenceNumber)
        {
            var index = sequenceNumber % bufferSize;
            if (sequence[index] == sequenceNumber)
            {
                return elements[index];
            }

            return default;
        }

        public TElement Allocate(ushort sequenceNumber)
        {
            if (sequenceNumber.IsSequenceLess((ushort) (lastSequence - bufferSize)))
            {
                return default;
            }

            if (lastSequence.IsSequenceLess((ushort) (sequenceNumber + 1)))
            {
                lastSequence = (ushort) (sequenceNumber + 1);
            }

            var index = sequenceNumber % bufferSize;
            sequence[index] = sequenceNumber;
            elements[index].Reset();
            return elements[index];
        }

        public ushort LastSequence => lastSequence;

        public void Reset()
        {
            for (var index = 0; index < bufferSize; ++index)
            {
                sequence[index] = default;
                elements[index].Reset();
            }

            lastSequence = 0;
        }

        public (ushort, int) GenerateAck()
        {
            int acksMask     = 0;
            for (ushort index = 0; index < sizeof(int) * 8; ++index)
            {
                if (Element((ushort) (lastSequence - index)) != null)
                {
                    acksMask |= 1 << index;
                }
            }

            return (lastSequence, acksMask);
        }
    }
}
