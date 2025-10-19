using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer.Models
{
    /// <summary>
    /// Unified request handler delegate - to group RouteCallback and MiddlewareCallback
    /// </summary>
    public delegate Task RequestHandler(HttpRequest request, HttpResponse response, Func<Task>? next);

    public static class RequestHandlerUtil
    {
        public static RequestHandler WrapToRequestHandler(RouteCallback routeCallback)
        {
            return async (request, response, next) =>
            {
                await routeCallback(request, response);
            };
        }

        public static RequestHandler WrapToRequestHandler(MiddlewareCallback middlewareCallback)
        {
            return async (request, response, next) =>
            {
                if (next == null)
                {
                    throw new ArgumentNullException(nameof(next), "Next function cannot be null for middleware.");
                }
                await middlewareCallback(request, response, next);
            };
        }
    }
}
