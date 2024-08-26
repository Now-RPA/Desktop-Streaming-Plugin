using DesktopStreaming.Core.Screenshot;
using DesktopStreaming.Core.Server;
using System;
using System.Net;

namespace DesktopStreaming
{
    [System.AddIn.AddIn("Desktop Streaming", Description = "Stream desktop over HTTP", Version = "2.0")]
    public class DesktopStreamer
    {
        private static StreamingServer _streamingServer;

        public static string StartStreaming(string ipAddress, int port, int fps = 60, bool displayCursor = true)
        {
            try
            {
                IPAddress ip = IPAddress.Parse(ipAddress);
                _streamingServer = StreamingServer.GetInstance(Resolution.Resolutions.Current, Fps.CreateInstance(fps), displayCursor);
                _streamingServer.Start(ip, port);

                string authKey = _streamingServer.GenerateAuthKey();
                string url = $"http://{ipAddress}:{port}/?auth={authKey}";
                return url;
            }
            catch (Exception ex)
            {
                throw new($"Unable to start streaming on port {port}: {ex.Message}", ex);
            }
        }

        public static void StopStreaming()
        {
            if (_streamingServer != null)
            {
                _streamingServer.Stop();
                _streamingServer = null;
            }
        }
    }
}