using System.Net;

namespace Lem.Networking.Utilities.Network
{
    public static class NetworkExtensions
    {
        public static int ToNetworkOrder(this int number)
        {
            return IPAddress.HostToNetworkOrder(number);
        }

        public static int ToHostOrder(this int number)
        {
            return IPAddress.NetworkToHostOrder(number);
        }
        
        public static uint ToNetworkOrder(this uint number)
        {
            return (uint)IPAddress.HostToNetworkOrder((int)number);
        }

        public static uint ToHostOrder(this uint number)
        {
            return (uint)IPAddress.NetworkToHostOrder((int)number);
        }
        
        public static short ToNetworkOrder(this short number)
        {
            return IPAddress.HostToNetworkOrder(number);
        }

        public static short ToHostOrder(this short number)
        {
            return IPAddress.NetworkToHostOrder(number);
        }
        
        public static ushort ToNetworkOrder(this ushort number)
        {
            return (ushort)IPAddress.HostToNetworkOrder((short)number);
        }

        public static ushort ToHostOrder(this ushort number)
        {
            return (ushort)IPAddress.NetworkToHostOrder((short)number);
        }
    }
}