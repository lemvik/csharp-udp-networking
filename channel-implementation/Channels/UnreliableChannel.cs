using System;

namespace Lem.Networking.Channels
{
    public class UnreliableChannel : IUnreliableChannel
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool Receive(in Span<byte> packetBuffer, out int receivedBytes)
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            throw new NotImplementedException();
        }

        public void Send(in ReadOnlySpan<byte> packetBuffer)
        {
            throw new NotImplementedException();
        }
    }
}