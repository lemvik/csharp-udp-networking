using System.Threading;
using System.Threading.Tasks;
using Lem.Networking.Utilities.Pools;

namespace Lem.Networking.Implementation.Connections.Transport
{
    public interface IConnectionTransport
    {
        Task Send(IMemoryResource packet, CancellationToken token = default);

        Task<IMemoryResource> Receive(CancellationToken token = default);

        bool TrySend(IMemoryResource packet);

        bool TryReceive(out IMemoryResource packet);

        ValueTask<bool> WaitUntilReceivable(CancellationToken token = default);
    }
}
