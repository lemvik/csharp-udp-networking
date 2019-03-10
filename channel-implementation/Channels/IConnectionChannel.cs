using Lem.Networking.Utilities.Pools;

namespace Lem.Networking.Implementation.Channels
{
    internal interface IConnectionChannel
    {
        void Receive(IMemoryResource packet);

        IMemoryResource PrepareToSend(int packetNumber, IMemoryResource packet);

        void Update();
    }
}
