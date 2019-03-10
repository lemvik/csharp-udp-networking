using System.Threading;
using System.Threading.Tasks;
using Lem.Networking.Utilities.Pools;

namespace Lem.Networking.Channels
{
    public interface IAckChannel : IChannel
    {
        ValueTask<int> Send(IMemoryResource packetContents, CancellationToken token = default);

        ValueTask<IMemoryResource> Receive(CancellationToken token = default);

        ValueTask<int> AwaitNextAck(CancellationToken token = default);

        bool TrySend(IMemoryResource packetContents, out int packetId);

        bool TryReceive(out IMemoryResource packetContents);

        bool TryGetNextAck(out int packetId);
    }
}
