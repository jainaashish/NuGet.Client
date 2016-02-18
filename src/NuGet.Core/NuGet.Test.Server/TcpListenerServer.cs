using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NuGet.Test.Server
{
    public class TcpListenerServer : ITestServer
    {
        public async Task<T> ExecuteAsync<T>(Func<string, Task<T>> action)
        {
            switch (Mode)
            {
                case TestServerMode.ServerProtocolViolation:
                    return await ExecuteAsync(
                        action,
                        StartServerProtocolViolationAsync);

                default:
                    throw new InvalidOperationException($"The mode {Mode} is not supported by this server.");
            }
        }

        public TestServerMode Mode { get; set; } = TestServerMode.ServerProtocolViolation;

        public string Content { get; set; } = @"{""foo"": ""bar""}";

        private async Task<T> ExecuteAsync<T>(Func<string, Task<T>> action, Func<TcpListener, CancellationToken, Task> startServer)
        {
            var portReserver = new PortReserver();
            return await portReserver.ExecuteAsync(
                async (port, token) =>
                {
                    // start the server
                    var serverCts = new CancellationTokenSource();
                    var tcpListener = new TcpListener(IPAddress.Loopback, port);
                    tcpListener.Start();
                    var serverTask = startServer(tcpListener, serverCts.Token);
                    var address = $"http://localhost:{port}/";

                    // execute the caller's action
                    var result = await action(address);

                    // stop the server
                    serverCts.Cancel();
                    tcpListener.Stop();

                    return result;
                },
                CancellationToken.None);
        }

        private async Task StartServerProtocolViolationAsync(TcpListener tcpListener, CancellationToken token)
        {
            // This server does not process any request body.
            while (!token.IsCancellationRequested)
            {
                using (var client = await Task.Run(tcpListener.AcceptTcpClientAsync, token))
                using (var stream = client.GetStream())
                using (var reader = new StreamReader(stream))
                using (var writer = new StreamWriter(stream, Encoding.ASCII))
                {
                    while (!string.IsNullOrEmpty(reader.ReadLine()))
                    {
                    }

                    writer.WriteLine("HTTP/1.1 BAD SERVER");
                    writer.WriteLine($"Date: {DateTimeOffset.UtcNow:R}");
                    writer.WriteLine();
                    writer.Flush();
                }
            }
        }
    }
}