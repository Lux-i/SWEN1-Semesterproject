using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using WebServer.Routing.Models;
using WebServer.Models;
using WebServer.Routing.Interfaces;

namespace WebServer.Routing
{
    /// <summary>
    /// Utility class to setup routes to be passed to the router
    /// Provides methods for registering routes with optional middleware
    /// </summary>
    class Routing
    {
        private RouteDictCollection _routes;
        private Router? _router;

        public Routing()
        {
            // init route dictionaries
            _routes = new RouteDictCollection
            {
                GetRoutes = new RouteDict(),
                PostRoutes = new RouteDict(),
                PutRoutes = new RouteDict(),
                DeleteRoutes = new RouteDict(),
                UpdateRoutes = new RouteDict()
            };
        }

        public void SetRouter(Router router)
        {
            _router = router;
        }

        public RouteDictCollection GetRoutes() => _routes;

        // Methods for registering routes
        #region Route Registration Methods
        public void Get(string path, RouteCallback callback, params MiddlewareCallback[] middleware)
        {
            _routes.GetRoutes[path] = callback;
            RegisterRouteMiddleware(path, middleware);
        }

        public void Get(IRoute route)
        {
            _routes.GetRoutes[route.Path] = route.Callback;
        }

        public void Post(string path, RouteCallback callback, params MiddlewareCallback[] middleware)
        {
            _routes.PostRoutes[path] = callback;
            RegisterRouteMiddleware(path, middleware);
        }

        public void Post(IRoute route)
        {
            _routes.PostRoutes[route.Path] = route.Callback;
        }

        public void Put(string path, RouteCallback callback, params MiddlewareCallback[] middleware)
        {
            _routes.PutRoutes[path] = callback;
            RegisterRouteMiddleware(path, middleware);
        }

        public void Put(IRoute route)
        {
            _routes.PutRoutes[route.Path] = route.Callback;
        }

        public void Delete(string path, RouteCallback callback, params MiddlewareCallback[] middleware)
        {
            _routes.DeleteRoutes[path] = callback;
            RegisterRouteMiddleware(path, middleware);
        }

        public void Delete(IRoute route)
        {
            _routes.DeleteRoutes[route.Path] = route.Callback;
        }

        public void Patch(string path, RouteCallback callback, params MiddlewareCallback[] middleware)
        {
            _routes.UpdateRoutes[path] = callback;
            RegisterRouteMiddleware(path, middleware);
        }

        public void Patch(IRoute route)
        {
            _routes.UpdateRoutes[route.Path] = route.Callback;
        }
        #endregion

        /// <summary>
        /// Private helper to register middleware for a route
        /// </summary>
        private void RegisterRouteMiddleware(string path, MiddlewareCallback[] middleware)
        {
            if (_router != null && middleware.Length > 0)
            {
                foreach (var mw in middleware)
                {
                    _router.UseForRoute(path, mw);
                }
            }
        }
    }
}
