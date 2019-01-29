using System;

namespace Lem.Networking.Implementation.Channels
{
    internal interface IChannel : IDisposable
    {
        ushort Epoch          { get; }
        ushort Sequence       { get; }
        int    MaxPayloadSize { get; }
        int    BufferRequiredByteSize(int desiredPacketByteSize);
    }
}