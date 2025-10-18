using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer.Routing.Interfaces
{
    public class IHttpResponse
    {
        public int StatusCode { get; private set; }
        public Dictionary<string, string> Headers { get; private set; }
        public string Body { get; private set; }
        public string ContentType { get; private set; }
    }
}
