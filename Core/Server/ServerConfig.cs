using System.Net;

namespace DesktopStreaming.Core.Server;

internal class ServerConfig(IPAddress ipAddress, int port)
{
    public IPAddress IpAddress { get; } = ipAddress;

    public int Port { get; } = port;
}