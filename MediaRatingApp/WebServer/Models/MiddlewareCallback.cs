using System;
using System.Threading.Tasks;

namespace WebServer.Models
{
    /// <summary>
    /// Delegate for middleware functions
    /// </summary>
    public delegate Task MiddlewareCallback(HttpRequest request, HttpResponse response, Func<Task> next);

    /// <summary>
    /// Represents a middleware with optional path filtering
    /// </summary>
    public class MiddlewareItem
    {
        public MiddlewareCallback Callback { get; set; }
        public string? PathPrefix { get; set; }

        public MiddlewareItem(MiddlewareCallback callback, string? pathPrefix = null)
        {
            Callback = callback;
            PathPrefix = pathPrefix;
        }

        /// <summary>
        /// Check if this middleware should run for the given path
        /// </summary>
        public bool ShouldRun(string path)
        {
            if (PathPrefix == null)
                return true;

            return path.StartsWith(PathPrefix, StringComparison.OrdinalIgnoreCase);
        }
    }
}
