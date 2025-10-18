using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebServer.Routing.Interfaces;

namespace WebServer.Routing.Models
{
    public class HttpRequest : IHttpRequest
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

        public dynamic CustomData { get; set; }

        public HttpRequest(IHttpRequest requestData)
        {
            Method = requestData.Method;
            Path = requestData.Path;
            FullPath = requestData.FullPath;
            Headers = requestData.Headers;
            Body = requestData.Body;
            QueryParameters = requestData.QueryParameters;
            PathParameters = requestData.PathParameters;
            Protocol = requestData.Protocol;
            IpAddress = requestData.IpAddress;
            ClientAgent = requestData.ClientAgent;
            CustomData = new ExpandoObject();
        }
    }
}
