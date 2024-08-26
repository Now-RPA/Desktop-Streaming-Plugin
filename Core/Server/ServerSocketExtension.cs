using System.Collections.Generic;
using System.Net.Sockets;

namespace DesktopStreaming.Core.Server
{
    internal static class ServerSocketExtension
    {
        public static IEnumerable<Socket> GetIncomingConnections(this Socket server)
        {
            while (true)
            {
                yield return server.Accept();
            }
        }
    }
}