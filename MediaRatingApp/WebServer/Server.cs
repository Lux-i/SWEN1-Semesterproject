using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using WebServer.Routing;

namespace WebServer
{
    class Server
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

        public void Start()
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
                HttpListenerContext context = _listener.GetContext();
                try
                {
                    _router.Route(context);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing request: {ex.Message}");
                    context.Response.StatusCode = 500;
                    context.Response.Close();
                }
            }
        }

        public void Stop()
        {
            _listener.Stop();
            _listener.Close();
            Console.WriteLine("Server stopped.");
        }
    }
}
