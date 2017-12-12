using LiteNetLib;
using System.Net;

namespace LunaCommon
{
    public static class ExtensionMethods
    {
        public static IPEndPoint GetEndpoint(this NetPeer value)
        {
            return new IPEndPoint(IPAddress.Parse(value.EndPoint.Host), value.EndPoint.Port);
        }
    }
}
