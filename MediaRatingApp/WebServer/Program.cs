using System.Net;

namespace MediaRatingApp.WebServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:80/");
            listener.Start();

            Console.WriteLine("Listening on http://localhost:80 ...");

            HttpListenerContext context = listener.GetContext();

            HttpListenerRequest request = context.Request;

            foreach (string header in request.Headers)
            {
                Console.WriteLine($"{header}: {request.Headers[header]}");
            }

            if(!request.HasEntityBody)
            {
                using var body = request.InputStream;
                using var reader = new System.IO.StreamReader(body, request.ContentEncoding);
                string s = reader.ReadToEnd();
                Console.WriteLine(s);
            }

            var response = context.Response;
            response.StatusCode = 200;
            response.ContentType = "text/html";
            string responseString = "<html><body>Hello World</body></html>";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();


            //Listen to HTTP
            /*
             socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Any, 80));
            socket.Listen(10);
             */
        }
    }
}
