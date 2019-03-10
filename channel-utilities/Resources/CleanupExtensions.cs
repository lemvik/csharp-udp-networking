using System;
using System.Threading.Channels;

namespace Lem.Networking.Utilities.Resources
{
    public static class CleanupExtensions
    {
        public static void CloseAndFlush<T>(this Channel<T> channel) where T : IDisposable
        {
            channel.Writer.Complete();

            while (channel.Reader.TryRead(out var item))
            {
                item.Dispose();
            }
        }
    }
}
