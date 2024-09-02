using DesktopStreaming.Authentication;
using DesktopStreaming.Core.Mjpeg;
using DesktopStreaming.Core.Video;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DesktopStreaming.Core.Server
{
    public sealed class StreamingServer : IDisposable
    {
        private static readonly Lazy<StreamingServer> LazyInstance =
            new(() => new StreamingServer(), LazyThreadSafetyMode.ExecutionAndPublication);

        public static StreamingServer Instance => LazyInstance.Value;

        private IEnumerable<Image> _images;
        private Socket _serverSocket;
        private Task _serverTask;
        private readonly IAuthenticationService _authService;
        private Fps Fps { get; set; }
        private Resolution.Resolutions Resolution { get; set; }
        private bool DisplayCursor { get; set; }
        private List<Task> ClientTasks { get; } = [];
        private volatile bool _isRunning;
        private string _currentUrl;
        private CancellationTokenSource _cts = new();

        private StreamingServer()
        {
            _authService = new SimpleAuthenticationService();
        }

        public string Start(IPAddress ipAddress, int port, Resolution.Resolutions resolution, Fps fps, bool displayCursor)
        {
            if (_isRunning)
            {
                Stop(); // Stop the existing stream if running
            }

            Resolution = resolution;
            Fps = fps;
            DisplayCursor = displayCursor;

            _images = Screenshot.TakeSeriesOfScreenshots(Resolution, DisplayCursor);

            var serverConfig = new ServerConfig(ipAddress, port);

            _serverTask = Task.Run(() => StartServerAsync(serverConfig));

            _isRunning = true;

            string authKey = _authService.GenerateAuthKey();
            _currentUrl = $"http://{ipAddress}:{port}/?auth={authKey}";

            return _currentUrl;
        }

        private async Task StartServerAsync(ServerConfig config)
        {
            try
            {
                _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _serverSocket.Bind(new IPEndPoint(config.IpAddress, config.Port));
                _serverSocket.Listen(10);

                while (_isRunning)
                {
                    try
                    {
                        Socket clientSocket = await Task.Factory.FromAsync(
                            _serverSocket.BeginAccept, _serverSocket.EndAccept, null);

                        var clientTask = HandleClientAsync(clientSocket, _cts.Token);

                        lock (ClientTasks)
                        {
                            ClientTasks.Add(clientTask);
                        }
                        CleanupCompletedTasks();
                    }
                    catch (SocketException)
                    {
                        // The server socket was closed, which is expected during shutdown
                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        // Socket was disposed, exit the loop
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error in server task: {ex.Message}");
                _isRunning = false;
            }
        }

        private void CleanupCompletedTasks()
        {
            lock (ClientTasks)
            {
                ClientTasks.RemoveAll(t => t.IsCompleted);
            }
        }

        private async Task HandleClientAsync(Socket clientSocket, CancellationToken cancellationToken)
        {
            Task currentTask;  // Placeholder task
            lock (ClientTasks)
            {
                currentTask = Task.Run(() => { }, cancellationToken); // Create a unique task for this client
                ClientTasks.Add(currentTask);
            }

            try
            {
                using (clientSocket)
                using (var stream = new NetworkStream(clientSocket, true))
                using (var reader = new StreamReader(stream))
                using (var writer = new StreamWriter(stream))
                {
                    writer.AutoFlush = true;
                    string request = await reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(request))
                    {
                        return;
                    }

                    string authKey = ParseAuthKey(request);

                    if (!_authService.ValidateAuthKey(authKey))
                    {
                        await writer.WriteLineAsync("HTTP/1.1 401 Unauthorized");
                        await writer.WriteLineAsync("Content-Type: text/plain");
                        await writer.WriteLineAsync();
                        await writer.WriteLineAsync("Invalid authentication key");
                        return;
                    }

                    using var mjpegWriter = new MjpegWriter(stream);
                    mjpegWriter.WriteHeaders();

                    foreach (var imgStream in _images.GetMjpegStream())
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await Task.Delay(Fps.Delay, cancellationToken);
                        mjpegWriter.WriteImage(imgStream);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation, no need to log
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client: {ex.Message}");
            }
            finally
            {
                lock (ClientTasks)
                {
                    ClientTasks.Remove(currentTask);
                }
            }
        }

        private string ParseAuthKey(string request)
        {
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

        public void Stop()
        {
            if (!_isRunning)
                return;

            _isRunning = false;
            _cts.Cancel();

            try
            {
                _serverSocket?.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception closing server socket: {ex.Message}");
            }

            // Wait for the server task to complete
            _serverTask?.Wait(TimeSpan.FromSeconds(5));

            // Wait for all client tasks to complete with a timeout
            Task.WaitAll([.. ClientTasks], TimeSpan.FromSeconds(5));

            ClientTasks.Clear();

            if (_images is IDisposable disposableImages)
            {
                disposableImages.Dispose();
            }

            _serverTask = null;
            _images = null;
            _serverSocket = null;
            _currentUrl = null;

            ResetCancellationTokenSource();
        }

        private void ResetCancellationTokenSource()
        {
            _cts.Dispose();
            _cts = new CancellationTokenSource();
        }

        public void Dispose()
        {
            Stop();
            _cts.Dispose();
        }
    }
}