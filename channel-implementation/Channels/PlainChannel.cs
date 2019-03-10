using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Lem.Networking.Channels;
using Lem.Networking.Implementation.Connections;
using Lem.Networking.Implementation.Encryption;
using Lem.Networking.Implementation.Packets;
using Lem.Networking.Utilities.Logging;
using Lem.Networking.Utilities.Memory;
using Lem.Networking.Utilities.Pools;
using Lem.Networking.Utilities.Resources;
using Lem.Networking.Utilities.Timing;

namespace Lem.Networking.Implementation.Channels
{
    internal class PlainChannel : IPlainChannel, IConnectionChannel
    {
        private readonly uint                                                                   connectionId;
        private readonly ushort                                                                 channelId;
        private readonly IFrameClock                                                            frameClock;
        private readonly IEncryption                                                            encryption;
        private readonly IMemoryPool                                                            memoryPool;
        private readonly ILogger                                                                logger;
        private readonly ChannelWriter<(ConnectionAction, IConnectionChannel, IMemoryResource)> sendingChannel;
        private readonly Channel<IMemoryResource>                                               receivingChannel;

        private readonly OnceOnlyGuard disposalGuard = new OnceOnlyGuard();

        private ChannelState sendingState;
        private ChannelState receivingState;

        internal PlainChannel(uint                                                                   connectionId,
                              ushort                                                                 channelId,
                              IFrameClock                                                            frameClock,
                              IEncryption                                                            encryption,
                              IMemoryPool                                                            memoryPool,
                              ILogger                                                                logger,
                              ChannelWriter<(ConnectionAction, IConnectionChannel, IMemoryResource)> sendingChannel)
        {
            this.connectionId   = connectionId;
            this.channelId      = channelId;
            this.frameClock     = frameClock;
            this.encryption     = encryption;
            this.memoryPool     = memoryPool;
            this.logger         = logger;
            this.sendingChannel = sendingChannel;
            this.receivingChannel = Channel.CreateUnbounded<IMemoryResource>(new UnboundedChannelOptions
            {
                SingleWriter                  = true,
                AllowSynchronousContinuations = false
            });
        }

        public void Dispose()
        {
            if (!disposalGuard.TryEntering)
            {
                return;
            }
            receivingChannel.CloseAndFlush();
        }

        public ValueTask Send(IMemoryResource packet, CancellationToken token = default)
        {
            return sendingChannel.WriteAsync((ConnectionAction.Send, this, packet), token);
        }

        public ValueTask<IMemoryResource> Receive(CancellationToken token = default)
        {
            return receivingChannel.Reader.ReadAsync(token);
        }

        public bool TrySend(IMemoryResource packet)
        {
            return sendingChannel.TryWrite((ConnectionAction.Send, this, packet));
        }

        public bool TryReceive(out IMemoryResource packet)
        {
            return receivingChannel.Reader.TryRead(out packet);
        }

        public void Receive(IMemoryResource incomingPacket)
        {
            using (incomingPacket)
            {
                var unreliablePacket = new UnreliablePacket(incomingPacket.Memory);
                var packetId         = unreliablePacket.PacketId;

                if (packetId.Id != channelId)
                {
                    logger.LogError("Channel received message addressed to another channel [packetId={}][channel={}]",
                                    packetId, this);
                    return;
                }

                if (!receivingState.RestoreRemoteState(packetId, out var remoteSendingState))
                {
                    logger.LogError("Channel is unable to restore state from remote channel [packetId={}][channel={}]",
                                    packetId,
                                    this);
                    return;
                }

                if (!unreliablePacket.Decrypt(encryption, receivingState.EncryptionNonce(packetId)))
                {
                    logger.LogError("Unable to decrypt incoming packet [packetId={}][channel={}]",
                                    packetId,
                                    this);
                    return;
                }

                if (remoteSendingState.Sequence < receivingState.Sequence)
                {
                    logger.LogDebug("Dropping stale remote packet [packetId={}][channel={}]",
                                    packetId,
                                    this);
                    return;
                }

                if (remoteSendingState.Sequence > receivingState.Sequence)
                {
                    receivingState.AdjustTo(remoteSendingState);
                }

                var result = memoryPool.AllocateCopy(unreliablePacket.PacketBody);
                if (!receivingChannel.Writer.TryWrite(result))
                {
                    result.Dispose();
                }
            }
        }

        public IMemoryResource PrepareToSend(int _, IMemoryResource packet)
        {
            using (packet)
            {
                if (!AllocatePacketId(out var sequence))
                {
                    logger.LogWarning("Failed to allocate sequence [channel={}]", this);
                    return null;
                }

                var sendBuffer = memoryPool.WithMemory(UnreliablePacket.EstimatedSize(packet.Memory));
                var unreliableMessage = new UnreliablePacket(sendBuffer.Memory)
                {
                    ConnectionId = connectionId,
                    PacketId     = sequence,
                    PacketBody   = packet.Memory
                };
                unreliableMessage.Encrypt(encryption, sendingState.EncryptionNonce(sequence));
                return sendBuffer;
            }
        }

        private bool AllocatePacketId(out PacketId result)
        {
            var messageTimestamp   = frameClock.CurrentFrame;
            var sendStateTimestamp = sendingState.Timestamp;

            if (messageTimestamp < sendStateTimestamp)
            {
                logger.LogWarning("Timestamps are assumed to monotonically increase [ID={}][now={}][sendState={}]",
                                  channelId,
                                  messageTimestamp,
                                  sendStateTimestamp);
                result = default;
                return false;
            }

            if (messageTimestamp - sendStateTimestamp > ChannelState.MaxTicksDiff)
            {
                logger.LogWarning("Timestamps are are too far away from each other [ID={}][now={}][sendState={}]",
                                  channelId,
                                  messageTimestamp,
                                  sendStateTimestamp);
                result = default;
                return false;
            }

            if (messageTimestamp == sendStateTimestamp)
            {
                if (!sendingState.AddAck())
                {
                    logger.LogWarning("Failed to allocate ack [ID={}][now={}][sendState={}]",
                                      channelId,
                                      messageTimestamp,
                                      sendStateTimestamp);
                    result = default;
                    return false;
                }
            }
            else
            {
                sendingState.AdjustTo(messageTimestamp);
            }

            result = new PacketId(channelId, sendingState.Ticks, sendingState.Acks);
            return true;
        }
    }
}
