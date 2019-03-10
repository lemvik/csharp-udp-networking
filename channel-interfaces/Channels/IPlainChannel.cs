using System.Threading;
using System.Threading.Tasks;
using Lem.Networking.Utilities.Pools;

namespace Lem.Networking.Channels
{
    public interface IPlainChannel : IChannel
    {
        ValueTask Send(IMemoryResource packet, CancellationToken token = default);

        ValueTask<IMemoryResource> Receive(CancellationToken token = default);

        bool TrySend(IMemoryResource packet);

        bool TryReceive(out IMemoryResource packet);
    }
}