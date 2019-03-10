using System;
using FluentAssertions;
using Lem.Networking.Exceptions;
using Lem.Networking.Implementation.Channels.Sync;
using Lem.Networking.Implementation.Packets;
using Xunit;
using Xunit.Abstractions;

namespace Lem.Networking.Tests.Channels
{
    public class AckChannelTests : IDisposable
    {
        private const    int               connectionId = 1;
        private const    byte              channelId    = 1;
        private readonly ITestOutputHelper outputHelper;
        private readonly AckChannel        ackChannel;

        public AckChannelTests(ITestOutputHelper outputHelper)
        {
            this.outputHelper = outputHelper;
            this.ackChannel   = new AckChannel(new ChannelAddress(connectionId, channelId), 1024);
        }

        public void Dispose()
        {
            ackChannel.Dispose();
        }

        [Fact]
        public void PacketSizeTest()
        {
            const int normalPacketSize = 10;

            ackChannel.BufferRequiredByteSize(normalPacketSize)
                      .Should()
                      .Be(normalPacketSize + AckPacketHeader.ByteSize,
                          "Packet should contain space for header and contents");

            var excessivePacketSize = ackChannel.MaxPayloadSize + 1;

            Action invalidAct = () => ackChannel.BufferRequiredByteSize(excessivePacketSize);
            invalidAct.Should().Throw<NetworkConstraintViolationException>()
                      .WithMessage($"Specified packet size + header exceeds maximum allowed packet size of {ackChannel.MaxPayloadSize}");

            const int emptyPacketSize = 0;

            ackChannel.BufferRequiredByteSize(emptyPacketSize)
                      .Should()
                      .Be(AckPacketHeader.ByteSize,
                          "Empty packet should consist of header only.");
        }

        [Fact]
        public void PacketStructureTest()
        {
            var payload      = new byte[] {1, 2, 3, 4};
            var packetBuffer = new byte[ackChannel.BufferRequiredByteSize(payload.Length)];
            Buffer.BlockCopy(payload, 0, packetBuffer, AckPacketHeader.ByteSize, payload.Length);
            var packetSpan = new Span<byte>(packetBuffer);

            var preparedPacket = ackChannel.PrepareSend(packetSpan);

            preparedPacket.Should().BeGreaterOrEqualTo(0);

            packetSpan.Read(out AckPacketHeader header).Should().BeTrue();

            header.ConnectionId.Should().Be(connectionId);
            header.ChannelId.Should().Be(channelId);
            header.Sequence.Should().Be((ushort)preparedPacket);
            header.Checksum.Should().Be(0);
            header.PayloadLength.Should().Be((ushort) payload.Length);
            header.ReceivedSequence.Should().Be(0);
            header.AcksMask.Should().Be(0);
        }
    }
}
