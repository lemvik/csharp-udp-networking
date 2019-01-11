using System;

namespace Lem.Networking.Implementation.Channels
{
    internal interface IChannel : IDisposable
    {
        int BufferRequiredByteSize(int desiredPacketByteSize);
    }
}