using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer.Routing.Interfaces
{
    public interface IHttpRequest
    {
        public HttpMethod Method { get; }
        public string Path { get; }
        public string FullPath { get; }
        public Dictionary<string, string> Headers { get; }
        public string Body { get; }
        public Dictionary<string, string> QueryParameters { get; }
        public Dictionary<string, string> PathParameters { get; }
        public string Protocol { get; }
        public string IpAddress { get; }
        public string ClientAgent { get; }
    }
}
