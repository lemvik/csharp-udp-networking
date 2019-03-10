using System;

namespace Rovio.Realtime.Network.Channels.Buffers
{
    internal class SendBufferMarker : BufferMarker
    {
        private readonly Action<uint> onAcknowledged;

        public SendBufferMarker(Action<uint> onAcknowledged)
        {
            this.onAcknowledged = onAcknowledged;
        }

        public void Acknowledged()
        {
            onAcknowledged(Sequence);
        }
    }
}