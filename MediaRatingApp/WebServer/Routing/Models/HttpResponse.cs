using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebServer.Routing.Interfaces;

namespace WebServer.Routing.Models
{
    public class HttpResponse : IHttpResponse
    {
        public int StatusCode { get; private set; }
        public Dictionary<string, string> Headers { get; private set; }
        public string Body { get; private set; }
        public string ContentType { get; private set; }

        public HttpResponse()
        {
            StatusCode = 200;
            Headers = new Dictionary<string, string>();
            Body = string.Empty;
            ContentType = "text/plain";
        }

        public HttpResponse SetStatusCode(int statusCode)
        {
            StatusCode = statusCode;
            return this;
        }

        public HttpResponse SetHeaders(Dictionary<string, string> headers)
        {
            Headers = headers;
            return this;
        }

        public void Send()
        {
            //use already set properties
            //send the response back to the client over http
        }

        public void Send(string body, int statusCode = 200, string contentType = "text/plain")
        {
            Body = body;
            StatusCode = statusCode;
            ContentType = contentType;
            //send the response
        }

        public void SendJson(string json, int statusCode = 200)
        {
            Body = json;
            StatusCode = statusCode;
            ContentType = "application/json";
            //send the response
        }

        public void SendHtml(string html, int statusCode = 200)
        {
            Body = html;
            StatusCode = statusCode;
            ContentType = "text/html";
            //send the response
        }
    }
}
