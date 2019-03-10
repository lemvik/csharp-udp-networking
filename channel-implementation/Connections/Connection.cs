using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Lem.Networking.Channels;
using Lem.Networking.Connections;
using Lem.Networking.Implementation.Channels;
using Lem.Networking.Implementation.Connections.Transport;
using Lem.Networking.Implementation.Packets;
using Lem.Networking.Utilities.Exceptions;
using Lem.Networking.Utilities.Pools;
using Lem.Networking.Utilities.Resources;

namespace Lem.Networking.Implementation.Connections
{
    public class Connection : IConnection
    {
        private readonly uint                                    connectionId;
        private readonly IConnectionTransport                    transport;
        private readonly IDictionary<ushort, IConnectionChannel> allocatedChannels;

        private readonly Channel<(ConnectionAction, IConnectionChannel, int, IMemoryResource)> actionPipeline;

        private readonly Channel<IMemoryResource> sendingChannel;
        private readonly CancellationTokenSource  controlTokenSource;
        private readonly TimeSpan                 updateInterval;
        private readonly Task                     updateTask;
        private readonly Task                     receivingTask;
        private readonly OnceOnlyGuard            disposeGuard = new OnceOnlyGuard();

        private ushort nextChannelId = 1;

        public Connection(uint                 connectionId,
                          IConnectionTransport transport,
                          CancellationToken    baseToken)
        {
            this.connectionId      = connectionId;
            this.transport         = transport;
            this.allocatedChannels = new Dictionary<ushort, IConnectionChannel>();
            this.sendingChannel = Channel.CreateUnbounded<IMemoryResource>(new UnboundedChannelOptions
            {
                SingleReader                  = true,
                AllowSynchronousContinuations = true
            });
            this.controlTokenSource = CancellationTokenSource.CreateLinkedTokenSource(baseToken);

            this.updateTask = Task.Run(() => Update(controlTokenSource.Token),
                                       controlTokenSource.Token);
            this.receivingTask = Task.Run(() => PumpReceivedPackets(controlTokenSource.Token),
                                          controlTokenSource.Token);
        }

        public IAckChannel AllocateAckChannel()
        {
            throw new System.NotImplementedException();
        }

        public IPlainChannel AllocatePlainChannel()
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            if (!disposeGuard.TryEntering)
            {
                return;
            }

            controlTokenSource.Cancel();
            sendingChannel.Writer.Complete();

            try
            {
                updateTask.Wait();
                updateTask.Dispose();
            }
            catch (AggregateException error)
            {
                if (!error.IsCancellation())
                {
                    throw;
                }
            }

            try
            {
                receivingTask.Wait();
                receivingTask.Dispose();
            }
            catch (AggregateException error)
            {
                if (!error.IsCancellation())
                {
                    throw;
                }
            }

            controlTokenSource.Dispose();
        }

        private async Task PumpReceivedPackets(CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var packet = await transport.Receive(token);
                    await actionPipeline.Writer.WriteAsync((ConnectionAction.Receive, null, 0, packet), token);
                }
                catch (AggregateException error)
                {
                    if (error.IsCancellation())
                    {
                        // TODO: notify about cancellation completion (not an error).
                        return;
                    }
                    else
                    {
                        // TODO: notify about an error.
                        return;
                    }
                }
                catch (Exception error)
                {
                    // TODO: mark connection as broken.
                    return;
                }
            }
        }

        private async Task Update(CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                var readable = await actionPipeline.Reader.WaitToReadAsync(token);

                if (!readable)
                {
                    // TODO: notify about pipeline closure.
                    return;
                }

                // This should never fail if above check completed.
                var (action, chan, packetNum, packet) = await actionPipeline.Reader.ReadAsync(token);

                using (packet)
                {
                    switch (action)
                    {
                        case ConnectionAction.Receive:
                        {
                            // In case of receiving we ignore chanId as it will be 0 (non-valid).
                            // We figure out channel that should receive packet using header.
                            var sequence  = PacketId.Peek(packet.Memory);
                            var channelId = sequence.Id;
                            if (allocatedChannels.TryGetValue(channelId, out var channel))
                            {
                                channel.Receive(packet);
                            }

                            break;
                        }
                        case ConnectionAction.Send:
                        {
                            var wrappedPacket = chan.PrepareToSend(packetNum, packet);
                            await transport.Send(wrappedPacket, token);
                            break;
                        }
                        case ConnectionAction.Update:
                        {
                            break;
                        }
                        default:
                            // TODO: notify about broken action.
                            continue;
                    }
                }
            }
        }
    }
}
