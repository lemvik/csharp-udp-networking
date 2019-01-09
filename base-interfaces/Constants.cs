using JetBrains.Annotations;

namespace Lem.Networking
{
    [PublicAPI]
    public static class Constants
    {
        /// <summary>
        /// Maximum size of the packet that could potentially be sent over any channel.
        /// </summary>
        public const int MaximumPacketSize = 1024;
    }
}