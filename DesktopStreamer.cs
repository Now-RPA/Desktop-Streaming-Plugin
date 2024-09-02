using DesktopStreaming.Core.Server;
using DesktopStreaming.Core.Video;
using System;
using System.AddIn;
using System.Net;

namespace DesktopStreaming;

[AddIn("Desktop Streaming")]
public class DesktopStreamer
{
    public static string StartStreaming(string ipAddress, int port, int fps = 60, bool displayCursor = true)
    {
        try
        {
            IPAddress ip = IPAddress.Parse(ipAddress);
            return StreamingServer.Instance.Start(ip, port, Resolution.Resolutions.Current, Fps.CreateInstance(fps), displayCursor);
        }
        catch (Exception ex)
        {
            throw new Exception($"Unable to start streaming on port {port}: {ex.Message}", ex);
        }
    }

    public static void StopStreaming()
    {
        StreamingServer.Instance.Stop();
    }
}