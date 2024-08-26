using DesktopStreaming.Core.Screenshot;
using DesktopStreaming.Core.Server;
using System;
using System.Net;
using static DesktopStreaming.Core.Screenshot.Resolution;

namespace DesktopStreaming
{
    public class DesktopStreamer
    {
        private static StreamingServer _streamingServer;

        public static string StartStreaming(string ipAddress, int port, Resolutions resolution = Resolutions.FullHd, double fps = 60, bool displayCursor = true)
        {
            IPAddress ip = IPAddress.Parse(ipAddress);

            // Try to start the server directly
            try
            {
                _streamingServer = StreamingServer.GetInstance(resolution, Fps.CreateInstance(fps), displayCursor);
                _streamingServer.Start(ip, port);

                string authKey = _streamingServer.GenerateAuthKey();
                string url = $"http://{ipAddress}:{port}/?auth={authKey}";
                Console.WriteLine($"Streaming started on {ipAddress}:{port} with resolution {GetResolutionDescription(resolution)} at {fps} FPS.");
                Console.WriteLine($"Streaming URL: {url}");
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
                _streamingServer.Dispose();
                _streamingServer = null;
                Console.WriteLine("Streaming stopped.");
            }
            else
            {
                Console.WriteLine("No active streaming to stop.");
            }
        }
    }
}