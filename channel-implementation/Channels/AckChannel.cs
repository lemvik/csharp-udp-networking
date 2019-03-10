using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Lem.Networking.Channels;
using Lem.Networking.Implementation.Channels.Buffers;
using Lem.Networking.Implementation.Connections;
using Lem.Networking.Implementation.Encryption;
using Lem.Networking.Implementation.Packets;
using Lem.Networking.Utilities.Logging;
using Lem.Networking.Utilities.Memory;
using Lem.Networking.Utilities.Pools;
using Rovio.Realtime.Network.Channels.Buffers;

namespace Lem.Networking.Implementation.Channels
{
    internal class AckChannel : IAckChannel, IConnectionChannel
    {
        private readonly uint                                                                   connectionId;
        private readonly ushort                                                                 channelId;
        private readonly IEncryption                                                            encryption;
        private readonly IMemoryPool                                                            memoryPool;
        private readonly ILogger                                                                logger;
        private readonly ChannelWriter<(ConnectionAction, IConnectionChannel, IMemoryResource)> sendingChannel;
        private readonly Channel<IMemoryResource>                                               receivingChannel;
        private readonly Channel<int>                                                           acksChannel;

        private readonly NetworkBuffer<BufferMarker> receiveBuffers;
        private readonly NetworkBuffer<BufferMarker> sendBuffers;

        private ChannelState receivingState;
        private ChannelState sendingState;

        private int packetNumber;

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public async ValueTask<int> Send(IMemoryResource packetContents, CancellationToken token = default)
        {
            var nextPacketNumber = Interlocked.Increment(ref packetNumber);

            await sendingChannel.WriteAsync((ConnectionAction.Send, this, packetContents), token);

            return nextPacketNumber;
        }

        public ValueTask<IMemoryResource> Receive(CancellationToken token = default)
        {
            return receivingChannel.Reader.ReadAsync(token);
        }

        public ValueTask<int> AwaitNextAck(CancellationToken token = default)
        {
            return acksChannel.Reader.ReadAsync(token);
        }

        public bool TrySend(IMemoryResource packetContents, out int packetId)
        {
            throw new System.NotImplementedException();
        }

        public bool TryReceive(out IMemoryResource packetContents)
        {
            return receivingChannel.Reader.TryRead(out packetContents);
        }

        public bool TryGetNextAck(out int packetId)
        {
            return acksChannel.Reader.TryRead(out packetId);
        }

        public void Receive(IMemoryResource packet)
        {
            using (packet)
            {
                var acknowledgingPacket = new AcknowledgingPacket(packet.Memory);
                var packetId            = acknowledgingPacket.PacketId;

                if (packetId.Id != channelId)
                {
                    logger.LogError("Channel received message addressed to another channel " +
                                    "[sequence={packetId}][channel={this}]", packetId, this);
                    return;
                }

                if (!receivingState.RestoreRemoteState(packetId, out var remoteState))
                {
                    logger.LogWarning("Unable to restore state from remote header [sequence={packetId}]", packetId);
                    return;
                }

                if (!acknowledgingPacket.Decrypt(encryption, receivingState.EncryptionNonce(packetId)))
                {
                    logger.LogWarning("Failed to decrypt incoming message [sequence={packetId}]", packetId);
                    return;
                }

                if (acknowledgingPacket.PacketBody.Length == 0)
                {
                    ReadRemoteAcknowledgements(acknowledgingPacket);
                    return;
                }

                pendingAck = true;

                if (receiveBuffers.Has(remoteState.Timestamp))
                {
                    logger.LogDebug("Received message that was received before, discarding " +
                                    "[sequence={packetId}][receiveState={receiveState}][remoteState={remoteState}]",
                                    packetId, receivingState, remoteState);
                    return;
                }

                if (receivingState.Timestamp < remoteState.Timestamp)
                {
                    receivingState.AdjustTo(remoteState);
                }

                receiveBuffers.Set(remoteState.Timestamp);
                receivingChannel.Writer.WriteAsync(memoryPool.AllocateCopy(acknowledgingPacket.PacketBody));
                ReadRemoteAcknowledgements(acknowledgingPacket);
            }
        }

        public IMemoryResource PrepareToSend(int packetNum, IMemoryResource packet)
        {
            pendingAck = false;
            var sequence      = AllocateSequence();
            var sendingBuffer = memoryPool.WithMemory(AcknowledgingPacket.EstimatedSize(outgoingMessage));
            var packet = new AcknowledgingPacket(sendingBuffer.Memory) {
                ConnectionId         = connectionId,
                PacketId=  sequence,
                LastReceivedSequence = receivingState.Timestamp,
                BufferedState        = receiveBuffers.BufferState(receiveState.Timestamp),
                PacketBody           = outgoingMessage
            };
            packet.Encrypt(encryption, sendingState.EncryptionNonce(sequence));
            packetsToSend.Add(sendingBuffer);
        }

        private void ReadRemoteAcknowledgements(AcknowledgingPacket packet)
        {
            var lastAckedRemote = packet.LastReceivedSequence;
            var ackMask         = packet.BufferedState;
            for (var index = 0; index < sizeof(uint) * 8; ++index)
            {
                if ((ackMask & 1) == 1)
                {
                    var seq = (uint) (lastAckedRemote - index);
                    sendBuffers.Unset(seq);
                    acksChannel.Writer.TryWrite((int) seq);
                }

                ackMask >>= 1;
            }
        }
    }
}
