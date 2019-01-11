using JetBrains.Annotations;

namespace Lem.Networking
{
    [PublicAPI]
    public static class Constants
    {
        public const int MaximumPayloadSizeBytes = 512;
        public const int PayloadLengthFieldSize = sizeof(ushort);
    }
}