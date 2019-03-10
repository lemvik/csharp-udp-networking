using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Lem.Networking.Utilities.Pools;

namespace Lem.Networking.Transport
{
    public interface INetworkTransport
    {
        Task SendTo(IPEndPoint endpoint, IMemoryResource memoryResource, CancellationToken token = default);

        Task<(IPEndPoint, IMemoryResource)> Receive(CancellationToken token = default);
    }
}
