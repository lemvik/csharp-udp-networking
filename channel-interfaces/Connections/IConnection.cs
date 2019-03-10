using System;
using Lem.Networking.Channels;

namespace Lem.Networking.Connections
{
    public interface IConnection : IDisposable
    {
        IAckChannel AllocateAckChannel();

        IPlainChannel AllocatePlainChannel();
    }
}
