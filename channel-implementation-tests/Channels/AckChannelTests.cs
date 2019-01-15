using System;
using FluentAssertions;
using Lem.Networking.Exceptions;
using Lem.Networking.Implementation.Channels;
using Lem.Networking.Implementation.Packets;
using Lem.Networking.Utilities.Memory;
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
                      .Be(normalPacketSize + AckChannel.AckChannelHeader.ByteSize,
                          "Packet should contain space for header and contents");

            var excessivePacketSize = ackChannel.MaxPayloadSize + 1;

            Action invalidAct = () => ackChannel.BufferRequiredByteSize(excessivePacketSize);
            invalidAct.Should().Throw<NetworkConstraintViolationException>()
                      .WithMessage($"Specified packet size + header exceeds maximum allowed packet size of {ackChannel.MaxPayloadSize}");

            const int emptyPacketSize = 0;

            ackChannel.BufferRequiredByteSize(emptyPacketSize)
                      .Should()
                      .Be(AckChannel.AckChannelHeader.ByteSize,
                          "Empty packet should consist of header only.");
        }

        [Fact]
        public void PacketStructureTest()
        {
            var payload      = new byte[] {1, 2, 3, 4};
            var packetBuffer = new byte[ackChannel.BufferRequiredByteSize(payload.Length)];
            Buffer.BlockCopy(payload, 0, packetBuffer, AckChannel.AckChannelHeader.ByteSize, payload.Length);
            var packetSpan = new Span<byte>(packetBuffer);

            var preparedPacket = ackChannel.PrepareSend(packetSpan);

            preparedPacket.Should().BeGreaterOrEqualTo(0);

            packetSpan.Int(0).Should().Be(connectionId);
            packetSpan.Byte(sizeof(int)).Should().Be(channelId);
            packetSpan.UShort(sizeof(int) + sizeof(byte)).Should().Be((ushort) payload.Length); // Payload length
            packetSpan.UShort(sizeof(int) + sizeof(byte) + sizeof(ushort))
                      .Should().Be((ushort) preparedPacket); // Sequence number
            packetSpan.UShort(sizeof(int) + sizeof(byte) + sizeof(ushort) * 2).Should().Be(0); // Recv sequence number
            packetSpan.Int(sizeof(int) + sizeof(byte) + sizeof(ushort) * 3).Should().Be(0); // Received packets mask
        }
    }
}