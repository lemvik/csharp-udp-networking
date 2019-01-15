using System;

namespace Lem.Networking.Implementation.Channels
{
    internal interface IChannel : IDisposable
    {
        int MaxPayloadSize { get; }

        int BufferRequiredByteSize(int desiredPacketByteSize);
    }
}