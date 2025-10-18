using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebServer.Routing.Models;
using WebServer.Models;

namespace WebServer.Routing
{
    class Router
    {
        private readonly RouteDictCollection _routes;
        private RouteCallback? _catchAllCallback;
        private readonly List<MiddlewareItem> _globalMiddleware;
        private readonly Dictionary<string, List<MiddlewareCallback>> _routeMiddleware;

        public Router(RouteDictCollection routes)
        {
            _routes = routes;
            _globalMiddleware = new List<MiddlewareItem>();
            _routeMiddleware = new Dictionary<string, List<MiddlewareCallback>>();
        }

        // Register Middleware
        public void Use(MiddlewareCallback middleware, string? pathPrefix = null)
        {
            _globalMiddleware.Add(new MiddlewareItem(middleware, pathPrefix));
        }

        // Register Route-Specific Middleware
        public void UseForRoute(string routePath, MiddlewareCallback middleware)
        {
            if (!_routeMiddleware.ContainsKey(routePath))
            {
                _routeMiddleware[routePath] = new List<MiddlewareCallback>();
            }
            _routeMiddleware[routePath].Add(middleware);
        }

        public async Task Route(HttpListenerContext context)
        {
            // Wrap into custom classes
            var request = new HttpRequest(context.Request);
            var response = new HttpResponse(context.Response);

            try
            {
                var applicableGlobalMiddleware = _globalMiddleware
                    .Where(m => m.ShouldRun(request.Path))
                    .Select(m => m.Callback)
                    .ToList();

                RouteDict? routeDict = GetRouteDictForMethod(request.Method);

                if (routeDict == null)
                {
                    response.SetStatusCode(405).Send("Method Not Allowed");
                    return;
                }

                // Match route
                var routeMatch = FindRoute(routeDict, request);
                RouteCallback? routeCallback = routeMatch.Callback;
                string? matchedRoutePath = routeMatch.Path;

                // Get route-specific middleware
                List<MiddlewareCallback> routeSpecificMiddleware = new List<MiddlewareCallback>();
                if (matchedRoutePath != null && _routeMiddleware.ContainsKey(matchedRoutePath))
                {
                    routeSpecificMiddleware = _routeMiddleware[matchedRoutePath];
                }

                var allMiddleware = applicableGlobalMiddleware
                    .Concat(routeSpecificMiddleware)
                    .ToList();

                // Execute middleware chain
                await ExecuteMiddlewareChain(allMiddleware, 0, request, response, async () =>
                {
                    // finalHandler to execute route callback / catch-all / response after middleware
                    if (routeCallback != null)
                    {
                        await routeCallback(request, response);
                    }
                    else if (_catchAllCallback != null)
                    {
                        await _catchAllCallback(request, response);
                    }
                    else
                    {
                        response.SetStatusCode(404).Send("Not Found");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing request: {ex.Message}");
                Console.WriteLine(ex.StackTrace);

                try
                {
                    if (response.Inner.OutputStream.CanWrite)
                    {
                        response.SetStatusCode(500).Send("Internal Server Error");
                    }
                }
                catch
                {
                    // Response already sent -> ignore
                }
            }
        }

        private async Task ExecuteMiddlewareChain(
            List<MiddlewareCallback> middleware,
            int index,
            HttpRequest request,
            HttpResponse response,
            Func<Task> finalHandler)
        {
            if (index >= middleware.Count)
            {
                await finalHandler();
                return;
            }

            var currentMiddleware = middleware[index];

            await currentMiddleware(request, response, async () =>
            {
                // Call the next middleware in the chain
                await ExecuteMiddlewareChain(middleware, index + 1, request, response, finalHandler);
            });

            // The middleware chain stops here if next() is not being called
        }

        /// <summary>
        /// Find a matching route in the dictionary
        /// </summary>
        private (RouteCallback? Callback, string? Path) FindRoute(RouteDict routeDict, HttpRequest request)
        {
            string path = request.Path;

            // Try exact matching
            if (routeDict.TryGetValue(path, out var callback))
            {
                return (callback, path);
            }

            // Try pattern matching for dynamic routes
            foreach (var route in routeDict)
            {
                if (TryMatchPattern(route.Key, path, out var pathParams))
                {
                    request.PathParameters = pathParams;
                    return (route.Value, route.Key);
                }
            }

            return (null, null);
        }

        /// <summary>
        /// Match a route pattern (e.g., "/api/user/:userId") with regex
        /// </summary>
        private bool TryMatchPattern(string pattern, string path, out Dictionary<string, string> pathParams)
        {
            pathParams = new Dictionary<string, string>();

            // Convert route pattern to regex
            string regexPattern = "^" + Regex.Escape(pattern)
                .Replace("\\:", ":")
                + "$";

            // Find parameter placeholders (e.g. :userId)
            var paramMatches = Regex.Matches(pattern, @":([a-zA-Z_][a-zA-Z0-9_]*)");
            foreach (Match match in paramMatches)
            {
                string paramName = match.Groups[1].Value;
                // Replace parameter placeholder with capture group
                regexPattern = regexPattern.Replace(":" + paramName, $"(?<{paramName}>[^/]+)");
            }

            var regex = new Regex(regexPattern);
            var pathMatch = regex.Match(path);

            if (!pathMatch.Success)
            {
                return false;
            }

            foreach (Match match in paramMatches)
            {
                string paramName = match.Groups[1].Value;
                if (pathMatch.Groups[paramName].Success)
                {
                    pathParams[paramName] = pathMatch.Groups[paramName].Value;
                }
            }

            return true;
        }

        /// <summary>
        /// Get the appropriate route dictionary for the HTTP method
        /// </summary>
        private RouteDict? GetRouteDictForMethod(HttpMethod method)
        {
            return method.Method.ToUpperInvariant() switch
            {
                "GET" => _routes.GetRoutes,
                "POST" => _routes.PostRoutes,
                "PUT" => _routes.PutRoutes,
                "DELETE" => _routes.DeleteRoutes,
                "PATCH" => _routes.UpdateRoutes,
                _ => null
            };
        }

        public void RegisterRoute(string method, string path, RouteCallback callback)
        {
            var httpMethod = new HttpMethod(method);
            var routeDict = GetRouteDictForMethod(httpMethod);

            if (routeDict == null)
            {
                throw new ArgumentException($"Unsupported HTTP method: {method}");
            }

            routeDict[path] = callback;
        }

        public void UnregisterRoute(string method, string path)
        {
            var httpMethod = new HttpMethod(method);
            var routeDict = GetRouteDictForMethod(httpMethod);
            routeDict?.Remove(path);
        }

        /// <summary>
        /// Register a catch-all callback for unmatched routes
        /// </summary>
        public void CatchAll(RouteCallback callback)
        {
            _catchAllCallback = callback;
        }
    }
}