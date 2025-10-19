using WebServer.Models;

namespace WebServer.Routing.Models
{
    public class Route
    {
        public string Path { get; set; }
        public string Method { get; set; } // Might be removed again from Route scope
        public List<RequestHandler> Callbacks { get; set; }

        public Route(string method, string path)
        {
            Method = method.ToUpperInvariant();
            Path = path;
            Callbacks = new List<RequestHandler>();
        }

        /// <summary>
        /// Add a callback to the route's callback list.
        /// Can be used to add middleware to the specific route and to add the endpoint (RouteCallback).
        /// </summary>
        public void Use(RequestHandler callback)
        {
            Callbacks.Add(callback);
        }

        /// <summary>
        /// Add a RouteCallback to the route's callback list.
        /// </summary>
        public void Use(RouteCallback routeCallback)
        {
            Callbacks.Add(RequestHandlerUtil.WrapToRequestHandler(routeCallback));
        }

        /// <summary>
        /// Add a MiddlewareCallback to the route's callback list.
        /// </summary>
        public void Use(MiddlewareCallback middlewareCallback)
        {
            Callbacks.Add(RequestHandlerUtil.WrapToRequestHandler(middlewareCallback));
        }
    }
}