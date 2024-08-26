using DesktopStreaming.Authentication;
using DesktopStreaming.Core.Mjpeg;
using DesktopStreaming.Core.Screenshot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace DesktopStreaming.Core.Server
{
    public sealed class StreamingServer : IDisposable
    {
        private static readonly object SyncRoot = new();
        private static StreamingServer _serverInstance;

        private readonly IEnumerable<Image> _images;
        private Socket _serverSocket;
        private Thread _thread;
        private readonly IAuthenticationService _authService;
        private Fps Fps { get; }
        private volatile bool _stopping;
        private volatile bool _disposed;
        private readonly object _stopLock = new();
        public List<Socket> Clients { get; }
        private bool IsRunning => _thread is { IsAlive: true };

        private StreamingServer(Resolution.Resolutions imageResolution, Fps fps, bool isDisplayCursor)
            : this(Screenshot.Screenshot.TakeSeriesOfScreenshots(imageResolution, isDisplayCursor), fps)
        {
        }

        private StreamingServer(IEnumerable<Image> images, Fps fps)
        {
            _thread = null;
            _images = images;

            Clients = [];
            Fps = fps;
            _authService = new SimpleAuthenticationService();
        }

        public static StreamingServer GetInstance(Resolution.Resolutions resolutions,
            Fps fps, bool isDisplayCursor)
        {
            lock (SyncRoot)
            {
                _serverInstance ??= new StreamingServer(resolutions, fps, isDisplayCursor);
            }

            return _serverInstance;
        }

        public void Start(IPAddress ipAddress, int port)
        {
            ThrowIfDisposed();
            var serverConfig = new ServerConfig(ipAddress, port);

            lock (this)
            {
                _thread = new Thread(StartServerThread)
                {
                    IsBackground = true
                };

                _thread.Start(serverConfig);
            }
        }

        private void StartServerThread(object config)
        {
            var serverConfig = (ServerConfig)config;

            try
            {
                _serverSocket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

                _serverSocket.Bind(new IPEndPoint(serverConfig.IpAddress,
                    serverConfig.Port));
                _serverSocket.Listen(10);

                foreach (var client in _serverSocket.GetIncomingConnections())
                {
                    ThreadPool.QueueUserWorkItem(StartClientThread, client);
                }
            }
            catch (SocketException)
            {
                foreach (var client in Clients.ToArray())
                {
                    try
                    {
                        client.Shutdown(SocketShutdown.Both);
                    }
                    catch (ObjectDisposedException)
                    {
                        client.Close();
                    }

                    Clients.Remove(client);
                }
            }
        }

        private void StartClientThread(object client)
        {
            var clientSocket = (Socket)client;
            clientSocket.SendTimeout = 10000;

            try
            {
                using var stream = new NetworkStream(clientSocket, true);
                using var reader = new StreamReader(stream);
                using var writer = new StreamWriter(stream);
                writer.AutoFlush = true;

                // Read the request line
                string request = reader.ReadLine();
                if (string.IsNullOrEmpty(request))
                {
                    return;
                }

                // Parse the auth key from the request
                string authKey = ParseAuthKey(request);

                if (!_authService.ValidateAuthKey(authKey))
                {
                    writer.WriteLine("HTTP/1.1 401 Unauthorized");
                    writer.WriteLine("Content-Type: text/plain");
                    writer.WriteLine();
                    writer.WriteLine("Invalid authentication key");
                    return;
                }

                Clients.Add(clientSocket);

                using var mjpegWriter = new MjpegWriter(stream);
                mjpegWriter.WriteHeaders();

                foreach (var imgStream in _images.GetMjpegStream())
                {
                    Thread.Sleep(Fps.Delay);
                    mjpegWriter.WriteImage(imgStream);
                }
            }
            catch (Exception)
            {
                // Handle or log the exception as needed
            }
            finally
            {
                try
                {
                    clientSocket.Shutdown(SocketShutdown.Both);
                }
                catch (ObjectDisposedException)
                {
                    clientSocket.Close();
                }

                lock (Clients)
                {
                    Clients.Remove(clientSocket);
                }
            }
        }

        private string ParseAuthKey(string request)
        {
            // Simple parsing of the auth key from the request
            // Assumes the format: "GET /?auth=key HTTP/1.1"
            int authIndex = request.IndexOf("auth=", StringComparison.Ordinal);
            if (authIndex == -1)
            {
                return null;
            }

            int keyStart = authIndex + 5;
            int keyEnd = request.IndexOf(' ', keyStart);
            if (keyEnd == -1)
            {
                keyEnd = request.Length;
            }

            return request.Substring(keyStart, keyEnd - keyStart);
        }

        public string GenerateAuthKey()
        {
            ThrowIfDisposed();
            return _authService.GenerateAuthKey();
        }

        public void Stop()
        {
            lock (_stopLock)
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(nameof(StreamingServer));
                }

                if (!IsRunning || _stopping)
                {
                    return;
                }

                _stopping = true;
            }

            try
            {
                _serverSocket?.Shutdown(SocketShutdown.Both);
            }
            catch (SocketException)
            {
                // Socket was already closed or another error occurred
            }
            finally
            {
                _serverSocket?.Close();
                _thread?.Join(TimeSpan.FromSeconds(5)); // Wait for the thread to finish

                _serverSocket = null;
                _thread = null;
                _stopping = false;
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Stop();

            foreach (var client in Clients)
            {
                try
                {
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                    client.Dispose();
                }
                catch (Exception)
                {
                    // Log or handle exception if needed
                }
            }

            Clients.Clear();

            if (_images is IDisposable disposableImages)
            {
                disposableImages.Dispose();
            }

            _disposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(StreamingServer));
            }
        }

        ~StreamingServer()
        {
            Dispose();
        }
    }
}