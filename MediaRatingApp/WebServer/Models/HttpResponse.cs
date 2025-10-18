using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebServer.Models
{
    /// <summary>
    /// Wrapper around HttpListenerResponse
    /// </summary>
    public class HttpResponse
    {
        private readonly HttpListenerResponse _innerResponse;
        public HttpListenerResponse Inner => _innerResponse;

        private bool _isSent = false;

        public HttpResponse(HttpListenerResponse response)
        {
            _innerResponse = response ?? throw new ArgumentNullException(nameof(response));

            // Defaults
            _innerResponse.StatusCode = 200;
            _innerResponse.ContentType = "text/plain";
        }

        // Methods to allow for chaining
        #region Chainable Setters
        public HttpResponse SetStatusCode(int statusCode)
        {
            EnsureNotSent();
            _innerResponse.StatusCode = statusCode;
            return this;
        }

        public HttpResponse SetContentType(string contentType)
        {
            EnsureNotSent();
            _innerResponse.ContentType = contentType;
            return this;
        }

        public HttpResponse SetHeader(string name, string value)
        {
            EnsureNotSent();
            _innerResponse.Headers[name] = value;
            return this;
        }

        public HttpResponse SetHeaders(Dictionary<string, string> headers)
        {
            EnsureNotSent();
            foreach (var header in headers)
            {
                _innerResponse.Headers[header.Key] = header.Value;
            }
            return this;
        }

        public HttpResponse SetCookie(Cookie cookie)
        {
            EnsureNotSent();
            _innerResponse.Cookies.Add(cookie);
            return this;
        }
        #endregion

        // Send methods including convenience send methods
        #region Send Methods
        public void Send()
        {
            EnsureNotSent();
            _isSent = true;
            _innerResponse.Close();
        }

        public void Send(string body, int statusCode = 200, string? contentType = null)
        {
            EnsureNotSent();
            _innerResponse.StatusCode = statusCode;
            if (contentType != null)
            {
                _innerResponse.ContentType = contentType;
            }
            WriteBody(body);
            _isSent = true;
            _innerResponse.Close();
        }

        public void SendJson(object obj, int statusCode = 200)
        {
            EnsureNotSent();
            _innerResponse.StatusCode = statusCode;
            _innerResponse.ContentType = "application/json";

            string json = System.Text.Json.JsonSerializer.Serialize(obj);
            WriteBody(json);
            _isSent = true;
            _innerResponse.Close();
        }

        public void SendHtml(string html, int statusCode = 200)
        {
            EnsureNotSent();
            _innerResponse.StatusCode = statusCode;
            _innerResponse.ContentType = "text/html; charset=utf-8";
            WriteBody(html);
            _isSent = true;
            _innerResponse.Close();
        }

        public void SendFile(string filePath, string? contentType = null)
        {
            EnsureNotSent();

            if (!System.IO.File.Exists(filePath))
            {
                _innerResponse.StatusCode = 404;
                WriteBody("File not found");
                _isSent = true;
                _innerResponse.Close();
                return;
            }

            // If content type is not provided, determine from file extension
            if (contentType == null)
            {
                contentType = GetContentTypeFromExtension(filePath);
            }
            _innerResponse.ContentType = contentType;

            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            _innerResponse.ContentLength64 = fileBytes.Length;
            _innerResponse.OutputStream.Write(fileBytes, 0, fileBytes.Length);

            _isSent = true;
            _innerResponse.Close();
        }

        public void Redirect(string url, int statusCode = 302)
        {
            EnsureNotSent();
            _innerResponse.StatusCode = statusCode;
            _innerResponse.Redirect(url);
            _isSent = true;
            _innerResponse.Close();
        }
        #endregion

        // Chainable response builders - for more convenience
        #region Convenience Builders
        public HttpResponse Status(int code)
        {
            return SetStatusCode(code);
        }

        public HttpResponse Type(string contentType)
        {
            return SetContentType(contentType);
        }

        public HttpResponse Header(string name, string value)
        {
            return SetHeader(name, value);
        }
        #endregion

        // Internal (private) helper methods
        #region Private Helpers
        private void WriteBody(string body)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(body);
            _innerResponse.ContentLength64 = buffer.Length;
            _innerResponse.OutputStream.Write(buffer, 0, buffer.Length);
        }

        private void EnsureNotSent()
        {
            if (_isSent)
            {
                throw new InvalidOperationException("Response has already been sent");
            }
        }

        private string GetContentTypeFromExtension(string filePath)
        {
            string extension = System.IO.Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".html" or ".htm" => "text/html",
                ".css" => "text/css",
                ".js" => "application/javascript",
                ".json" => "application/json",
                ".xml" => "application/xml",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".svg" => "image/svg+xml",
                ".pdf" => "application/pdf",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
        }
        #endregion
    }
}