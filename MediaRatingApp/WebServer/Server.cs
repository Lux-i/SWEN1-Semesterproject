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
        private HttpListener listener;
        private Router router;

        public Server(string[] prefixes, Router router)
        {
            listener = new HttpListener();
            foreach (string prefix in prefixes)
            {
                listener.Prefixes.Add(prefix);
            }
            this.router = router;
        }

        public void Start()
        {
            listener.Start();
            Console.WriteLine("Server started. Listening on the following prefixes:");
            foreach (string prefix in listener.Prefixes)
            {
                Console.WriteLine(prefix);
            }
            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                try
                {
                    router.Route(context);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing request: {ex.Message}");
                    context.Response.StatusCode = 500;
                    context.Response.Close();
                }
            }
        }
    }
}
