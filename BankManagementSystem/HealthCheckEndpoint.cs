using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BankManagementSystem
{
    /// <summary>
    /// Health check endpoint for containerization support
    /// Provides HTTP endpoint at /health for container orchestration
    /// </summary>
    public class HealthCheckEndpoint : IDisposable
    {
        private readonly int _port;
        private HttpListener? _listener;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _listenerTask;

        public HealthCheckEndpoint(int port)
        {
            _port = port;
        }

        public void Start()
        {
            try
            {
                _listener = new HttpListener();
                string prefix = $"http://+:{_port}/";
                _listener.Prefixes.Add(prefix);
                _listener.Start();

                _cancellationTokenSource = new CancellationTokenSource();
                _listenerTask = Task.Run(() => ListenAsync(_cancellationTokenSource.Token));

                Console.WriteLine($"Health check endpoint started on port {_port}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start health check endpoint: {ex.Message}");
            }
        }

        private async Task ListenAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && _listener != null && _listener.IsListening)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    await HandleRequestAsync(context);
                }
                catch (HttpListenerException)
                {
                    // Listener stopped
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error handling health check request: {ex.Message}");
                }
            }
        }

        private async Task HandleRequestAsync(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            // Handle health check endpoint
            if (request.Url != null && request.Url.AbsolutePath.Equals("/health", StringComparison.OrdinalIgnoreCase))
            {
                var healthStatus = new
                {
                    status = "UP",
                    timestamp = DateTime.UtcNow.ToString("o"),
                    application = "BankManagementSystem",
                    version = "1.0.0"
                };

                string jsonResponse = Newtonsoft.Json.JsonConvert.SerializeObject(healthStatus);
                byte[] buffer = Encoding.UTF8.GetBytes(jsonResponse);

                response.ContentType = "application/json";
                response.ContentLength64 = buffer.Length;
                response.StatusCode = 200;

                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                response.OutputStream.Close();
            }
            else
            {
                response.StatusCode = 404;
                response.Close();
            }
        }

        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
            _listener?.Stop();
            _listenerTask?.Wait(TimeSpan.FromSeconds(5));
        }

        public void Dispose()
        {
            Stop();
            _listener?.Close();
            _cancellationTokenSource?.Dispose();
        }
    }
}
