using System;
using System.Net;
using System.Text;
using System.Threading;

namespace BankManagementSystem
{
    /// <summary>
    /// Provides a simple HTTP health check endpoint for containerization
    /// </summary>
    public class HealthCheckEndpoint
    {
        private HttpListener _listener;
        private Thread _listenerThread;
        private bool _isRunning;

        public void Start()
        {
            if (_isRunning)
                return;

            string healthCheckPort = Environment.GetEnvironmentVariable("HEALTH_CHECK_PORT") ?? "8080";
            string prefix = $"http://+:{healthCheckPort}/health/";

            _listener = new HttpListener();
            _listener.Prefixes.Add(prefix);

            try
            {
                _listener.Start();
                _isRunning = true;

                _listenerThread = new Thread(ListenForRequests)
                {
                    IsBackground = true
                };
                _listenerThread.Start();

                Console.WriteLine($"Health check endpoint started on port {healthCheckPort}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start health check endpoint: {ex.Message}");
            }
        }

        public void Stop()
        {
            if (!_isRunning)
                return;

            _isRunning = false;
            _listener?.Stop();
            _listener?.Close();
        }

        private void ListenForRequests()
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
                string responseString = "{\"status\":\"healthy\",\"service\":\"BankManagementSystem\"}";
                byte[] buffer = Encoding.UTF8.GetBytes(responseString);

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
