using DesktopStreaming.Core.Screenshot;
using DesktopStreaming.Core.Server;
using System;
using System.Net;
using System.Threading;

namespace DesktopStreaming
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Desktop Streaming Server Test");
            Console.WriteLine("-----------------------------");

            // Set up server configuration
            string ipAddress = "127.0.0.1";
            int port = 8080;
            double fps = 120.0;
            bool displayCursor = true;

            // Resolution selection
            Resolution.Resolutions selectedResolution = SelectResolution();

            Console.WriteLine($"Server IP: {ipAddress}");
            Console.WriteLine($"Server Port: {port}");
            Console.WriteLine($"FPS: {fps}");
            Console.WriteLine($"Display Cursor: {displayCursor}");
            Console.WriteLine($"Selected Resolution: {Resolution.GetResolutionDescription(selectedResolution)}");
            Console.WriteLine();

            // Create Fps instance
            var fpsInstance = Fps.CreateInstance(fps);

            // Create and start the streaming server
            using (var server = StreamingServer.GetInstance(selectedResolution, fpsInstance, displayCursor))
            {
                try
                {
                    server.Start(IPAddress.Parse(ipAddress), port);
                    Console.WriteLine("Server started successfully.");

                    // Generate and display auth key
                    string authKey = server.GenerateAuthKey();
                    Console.WriteLine($"Auth Key: {authKey}");
                    Console.WriteLine($"Use this URL to connect: http://{ipAddress}:{port}/?auth={authKey}");

                    Console.WriteLine("\nPress 'Q' to stop the server and exit.");

                    // Keep the server running until user wants to quit
                    while (true)
                    {
                        if (Console.KeyAvailable)
                        {
                            var key = Console.ReadKey(true).Key;
                            if (key == ConsoleKey.Q)
                            {
                                break;
                            }
                        }

                        // Optional: You can add periodic status updates here
                        Console.WriteLine($"Active clients: {server.Clients.Count}");

                        Thread.Sleep(1000); // Wait for 5 seconds before next status update
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
                finally
                {
                    Console.WriteLine("Stopping the server...");
                    server.Stop();
                    Console.WriteLine("Server stopped.");
                }
            }

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        static Resolution.Resolutions SelectResolution()
        {
            Console.WriteLine("Select a resolution:");
            for (int i = 0; i < Enum.GetNames(typeof(Resolution.Resolutions)).Length; i++)
            {
                var resolution = (Resolution.Resolutions)i;
                Console.WriteLine($"{i + 1}. {Resolution.GetResolutionDescription(resolution)}");
            }

            while (true)
            {
                Console.Write($"Enter your choice (1-{Enum.GetNames(typeof(Resolution.Resolutions)).Length}): ");
                if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 1 && choice <= Enum.GetNames(typeof(Resolution.Resolutions)).Length)
                {
                    return (Resolution.Resolutions)(choice - 1);
                }
                else
                {
                    Console.WriteLine($"Invalid input. Please enter a number between 1 and {Enum.GetNames(typeof(Resolution.Resolutions)).Length}.");
                }
            }
        }
    }
}