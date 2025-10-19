using System;
using System.Threading.Tasks;

namespace WebServer.Models
{
    /// <summary>
    /// Delegate for middleware functions
    /// </summary>
    public delegate Task MiddlewareCallback(HttpRequest request, HttpResponse response, Func<Task> next);
}
