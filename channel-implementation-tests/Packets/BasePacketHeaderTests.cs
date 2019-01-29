using System;
using FluentAssertions;
using Lem.Networking.Implementation.Packets;
using Xunit;
using Xunit.Abstractions;

namespace Lem.Networking.Tests.Packets
{
    public class BasePacketHeaderTests : IDisposable
    {
        private readonly ITestOutputHelper outputHelper;

        public BasePacketHeaderTests(ITestOutputHelper outputHelper)
        {
            this.outputHelper = outputHelper;
        }

        public void Dispose()
        {
        }

        [Fact]
        public void TestGetters()
        {
            // This is in network order - big endian.
            var sourceBytes = new byte[] {
                0, 1,       // ushort connectionId == 1
                1,          // byte   channelId == 1
                0, 15,      // ushort sequenceId == 15
                1, 1, 1, 1, // uint   checksum == 16843009
                0, 2,       // ushort payloadLength == 0
                
                1, 2        // payload - not used.
            };

            var sourceSpan = new ReadOnlySpan<byte>(sourceBytes);
            sourceSpan.Read(out BasePacketHeader header).Should().BeTrue();
            header.ConnectionId.Should().Be(1);
            header.ChannelId.Should().Be(1);
            header.Sequence.Should().Be(15);
            header.Checksum.Should().Be(16843009);
            header.PayloadLength.Should().Be(2);
        }

        [Fact]
        public void TestSetters()
        {
            var targetBytes = new byte[BasePacketHeader.ByteSize];
            var targetSpan = new Span<byte>(targetBytes);
            targetSpan.Write(new BasePacketHeader {
                ConnectionId  = 1,
                ChannelId     = 1,
                Sequence      = 15,
                Checksum      = 16843009,
                PayloadLength = 2
            }).Should().BeTrue();

            targetBytes.Should().Equal(0, 1, 
                                       1, 
                                       0, 15, 
                                       1, 1, 1, 1, 
                                       0, 2);
        }
    }
}