using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using WebServer.Routing;

namespace WebServer
{
    public class Server
    {
        private HttpListener _listener;
        private Router _router;

        public Server(string[] prefixes, Router router)
        {
            _listener = new HttpListener();
            foreach (string prefix in prefixes)
            {
                _listener.Prefixes.Add(prefix);
            }
            _router = router;
        }

        ~Server()
        {
            _listener.Close();
        }

        public async Task Start()
        {
            _listener.Start();
            Console.WriteLine("Server started. Listening on:");
            Console.ForegroundColor = ConsoleColor.Cyan;
            foreach (string prefix in _listener.Prefixes)
            {
                Console.WriteLine($"*  {prefix}");
            }
            Console.ResetColor();
            Console.WriteLine("");
            while (true)
            {
                HttpListenerContext context = await _listener.GetContextAsync();
                _ = HandleRequestAsync(context);
            }
        }

        private async Task HandleRequestAsync(HttpListenerContext context)
        {
            try
            {
                await _router.Route(context);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error processing request:");
                Console.WriteLine($"{ex.Message}\n");
                Console.ResetColor();

                // Try to send a 500 response
                try
                {
                    context.Response.StatusCode = 500;
                    context.Response.Close();
                }
                catch
                { }
            }
        }

        public void Stop()
        {
            _listener.Stop();
            Console.WriteLine("Server stopped.");
        }
    }
}
