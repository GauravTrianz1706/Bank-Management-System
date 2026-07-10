using System;
using System.Net;
using System.Text;
using System.Threading;

namespace BankManagementSystem
{
    /// <summary>
    /// Provides a simple HTTP health check endpoint for containerization.
    /// This endpoint is required for Kubernetes liveness and readiness probes.
    /// Listens on port 8080 by default (configurable via HEALTH_CHECK_PORT environment variable).
    /// 
    /// Endpoints:
    /// - /health - Returns JSON health status
    /// - /healthz - Returns JSON health status (Kubernetes convention)
    /// 
    /// Response format:
    /// {
    ///   "status": "healthy",
    ///   "timestamp": "2024-01-01T00:00:00.0000000Z",
    ///   "application": "BankManagementSystem",
    ///   "version": "1.0.0"
    /// }
    /// </summary>
    public class HealthCheckEndpoint
    {
        private HttpListener _listener;
        private Thread _listenerThread;
        private bool _isRunning;
        private readonly int _port;

        public HealthCheckEndpoint()
        {
            // Get port from environment variable or use default
            string portEnv = Environment.GetEnvironmentVariable("HEALTH_CHECK_PORT");
            _port = int.TryParse(portEnv, out int port) ? port : 8080;
        }

        public void Start()
        {
            if (_isRunning)
                return;

            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://+:{_port}/health/");
            _listener.Prefixes.Add($"http://+:{_port}/healthz/");
            
            try
            {
                _listener.Start();
                _isRunning = true;

                _listenerThread = new Thread(HandleRequests)
                {
                    IsBackground = true,
                    Name = "HealthCheckListener"
                };
                _listenerThread.Start();

                Console.WriteLine($"Health check endpoint started on port {_port}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start health check endpoint: {ex.Message}");
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _listener?.Stop();
            _listener?.Close();
        }

        private void HandleRequests()
        {
            while (_isRunning)
            {
                try
                {
                    var context = _listener.GetContext();
                    ProcessRequest(context);
                }
                catch (HttpListenerException)
                {
                    // Listener was stopped
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Health check error: {ex.Message}");
                }
            }
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            try
            {
                var response = context.Response;
                
                // Simple health check response
                var healthStatus = new
                {
                    status = "healthy",
                    timestamp = DateTime.UtcNow.ToString("o"),
                    application = "BankManagementSystem",
                    version = "1.0.0"
                };

                string jsonResponse = Newtonsoft.Json.JsonConvert.SerializeObject(healthStatus);
                byte[] buffer = Encoding.UTF8.GetBytes(jsonResponse);

                response.ContentType = "application/json";
                response.ContentLength64 = buffer.Length;
                response.StatusCode = 200;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.OutputStream.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing health check request: {ex.Message}");
            }
        }
    }
}
