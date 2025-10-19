using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebServer.Models
{
    /// <summary>
    /// Wrapper around HttpListenerRequest
    /// </summary>
    public class HttpRequest
    {
        private readonly HttpListenerRequest _innerRequest;
        public HttpListenerRequest Inner => _innerRequest;

        // Convenience properties
        #region Convenience Properties
        public HttpMethod Method => new HttpMethod(_innerRequest.HttpMethod);
        public string Path { get; set; }
        public string OriginalPath { get; } // If original path is needed in nested routing
        public string FullPath => _innerRequest.Url?.PathAndQuery ?? string.Empty;
        public string Protocol => _innerRequest.ProtocolVersion.ToString();
        public string IpAddress => _innerRequest.RemoteEndPoint?.Address.ToString() ?? string.Empty;
        public string ClientAgent => _innerRequest.UserAgent ?? string.Empty;
        #endregion

        private Dictionary<string, string>? _headers;
        public Dictionary<string, string> Headers
        {
            get
            {
                if (_headers == null)
                {
                    _headers = new Dictionary<string, string>();
                    foreach (string key in _innerRequest.Headers.AllKeys)
                    {
                        if (key != null)
                        {
                            _headers[key] = _innerRequest.Headers[key] ?? string.Empty;
                        }
                    }
                }
                return _headers;
            }
        }

        private string? _body;
        public string Body
        {
            get
            {
                if (_body == null && _innerRequest.HasEntityBody)
                {
                    using var reader = new System.IO.StreamReader(
                        _innerRequest.InputStream,
                        _innerRequest.ContentEncoding);
                    _body = reader.ReadToEnd();
                }
                return _body ?? string.Empty;
            }
        }

        private Dictionary<string, string>? _queryParameters;
        public Dictionary<string, string> QueryParameters
        {
            get
            {
                if (_queryParameters == null)
                {
                    _queryParameters = new Dictionary<string, string>();
                    var queryString = _innerRequest.QueryString;
                    foreach (string key in queryString.AllKeys)
                    {
                        if (key != null)
                        {
                            _queryParameters[key] = queryString[key] ?? string.Empty;
                        }
                    }
                }
                return _queryParameters;
            }
        }

        public Dictionary<string, string> PathParameters { get; set; }

        // Allows for attachment of custom data (e.g. from middleware)
        public dynamic CustomData { get; set; }

        public HttpRequest(HttpListenerRequest request)
        {
            _innerRequest = request ?? throw new ArgumentNullException(nameof(request));

            Path = _innerRequest.Url?.AbsolutePath ?? "/";
            OriginalPath = Path;

            PathParameters = new Dictionary<string, string>();
            CustomData = new ExpandoObject();
        }

        // Convenience methods
        #region Convenience Methods
        public string GetHeader(string name) => Headers.TryGetValue(name, out var value) ? value : string.Empty;

        public string GetQueryParam(string name) => QueryParameters.TryGetValue(name, out var value) ? value : string.Empty;

        public string GetPathParam(string name) => PathParameters.TryGetValue(name, out var value) ? value : string.Empty;

        public bool HasHeader(string name) => Headers.ContainsKey(name);

        public bool HasQueryParam(string name) => QueryParameters.ContainsKey(name);

        public bool HasPathParam(string name) => PathParameters.ContainsKey(name);
        #endregion
    }
}