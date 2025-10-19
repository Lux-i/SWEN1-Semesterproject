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
        #region Route Dictionaries
        private readonly RouteDict _getRoutes;
        private readonly RouteDict _postRoutes;
        private readonly RouteDict _putRoutes;
        private readonly RouteDict _deleteRoutes;
        private readonly RouteDict _patchRoutes;
        private readonly RouteDict _updateRoutes;
        #endregion
        private readonly List<MiddlewareCallback> _middleware; // Global middleware of this router
        private Route? _catchAll; // Might be removed - See CatchAll()

        public Router()
        {
            #region Initialize Route Dictionaries
            _getRoutes = new RouteDict();
            _postRoutes = new RouteDict();
            _putRoutes = new RouteDict();
            _deleteRoutes = new RouteDict();
            _patchRoutes = new RouteDict();
            _updateRoutes = new RouteDict();
            #endregion

            _middleware = new List<MiddlewareCallback>();
        }

        /// <summary>
        /// Register given middleware 'globally' to the router
        /// </summary>
        public void Use(MiddlewareCallback middleware)
        {
            _middleware.Add(middleware);
        }

        public void Use(string pathPrefix, Router subRouter)
        {
            Use(async (req, res, next) =>
            {
                if (req.Path.StartsWith(pathPrefix))
                {
                    string originalPath = req.Path;
                    req.Path = req.Path.Substring(pathPrefix.Length);
                    if (string.IsNullOrEmpty(req.Path))
                    {
                        req.Path = "/";
                    }

                    await subRouter.Route(req, res);

                    req.Path = originalPath; // Restore original path
                }
                else
                {
                    // Continue - Not for this sub-router
                    if (next != null)
                    {
                        await next();
                    }
                }
            });
        }

        #region Route Management

        /// <summary>
        /// Add a predefined Route instance
        /// </summary>
        public void AddRoute(Route route)
        {
            try
            {
                RouteDict routeDict = GetRouteDict(route.Method);
                routeDict[route.Path] = route;
            }
            catch (ArgumentException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Unsupported HTTP method: {route.Method}");
                Console.WriteLine($"Route {route.Path} not added.");
                Console.ResetColor();
            }
        }

        private void CreateAndAddRoute(string method, string path, object[] callbacks)
        {
            try
            {
                Route route = CreateRoute(method, path, callbacks);
                AddRoute(route);
            }
            catch (ArgumentException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error adding {method} route {path}: {ex.Message}");
                Console.ResetColor();
            }
        }

        // Add routes by providing method, path, and callback(s)
        public void Get(string path, params object[] callbacks)
        {
            CreateAndAddRoute("GET", path, callbacks);
        }

        public void Post(string path, params object[] callbacks)
        {
            CreateAndAddRoute("POST", path, callbacks);
        }

        public void Put(string path, params object[] callbacks)
        {
            CreateAndAddRoute("PUT", path, callbacks);
        }

        public void Delete(string path, params object[] callbacks)
        {
            CreateAndAddRoute("DELETE", path, callbacks);
        }

        public void Patch(string path, params object[] callbacks)
        {
            CreateAndAddRoute("PATCH", path, callbacks);
        }

        public void Update(string path, params object[] callbacks)
        {
            CreateAndAddRoute("UPDATE", path, callbacks);
        }

        /// <summary>
        /// Remove a route by method and path
        /// </summary>
        public void Unregister(string method, string path)
        {
            try
            {
                RouteDict routeDict = GetRouteDict(method);
                routeDict.Remove(path);
            }
            catch (ArgumentException)
            {
                // Unsupported method -> ignore
            }
        }

        /// <summary>
        /// Register a catch-all callback for unmatched routes
        /// Might be removed -> if removed use a wildcard route at the end instead "*"
        /// </summary>
        public void CatchAll(RouteCallback callback)
        {
            _catchAll = new Models.Route("*", "*");
            _catchAll.Use(callback);
        }

        /// <summary>
        /// Check if a route exists
        /// </summary>
        public bool HasRoute(string method, string path)
        {
            try
            {
                RouteDict routeDict = GetRouteDict(method);
                return routeDict.ContainsKey(path);
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        #endregion

        #region Routing

        public async Task Route(HttpListenerContext context)
        {
            // Wrap into custom classes
            var request = new HttpRequest(context.Request);
            var response = new HttpResponse(context.Response);

            await Route(request, response);
        }

        public async Task Route(HttpRequest request, HttpResponse response)
        {
            try
            {
                // Match route
                var matchedRoute = FindRoute(request);

                if (matchedRoute != null)
                {
                    await ExecuteCallbacks(request, response, matchedRoute);
                }
                else if (_catchAll != null)
                {
                    await ExecuteCallbacks(request, response, _catchAll);
                }
                else
                {
                    response.SetStatusCode(404).Send("Not Found");
                }
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

        /// <summary>
        /// Find a matching route in the dictionary
        /// </summary>
        private Route? FindRoute(HttpRequest request)
        {
            try
            {
                string path = request.Path;
                RouteDict routeDict = GetRouteDict(request.Method.Method);

                #region Route Matching Logic

                // Try with exact match first
                if (routeDict.TryGetValue(path, out var route))
                {
                    return route;
                }

                // Try with pattern match
                foreach (var pair in routeDict)
                {
                    if (TryMatchPattern(pair.Key, path, out var pathParams))
                    {
                        // Add path parameters to request object
                        request.PathParameters = pathParams;
                        return route;
                    }
                }

                #endregion
            }
            catch (ArgumentException)
            {
                return null; // Unsupported method
            }

            return null;
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

        #endregion

        #region Callback execution

        private async Task ExecuteCallbacks(HttpRequest request, HttpResponse response, Route route)
        {
            List<RequestHandler> pipeline = new List<RequestHandler>();
            pipeline.AddRange(_middleware.Select(mw => RequestHandlerUtil.WrapToRequestHandler(mw)));
            pipeline.AddRange(route.Callbacks);

            await ExecuteCallbackChain(0, pipeline, request, response);
        }

        private async Task ExecuteCallbackChain(int index, List<RequestHandler> pipeline,
            HttpRequest request, HttpResponse response)
        {
            if (index >= pipeline.Count)
            {
                return; // End of chain
            }

            RequestHandler callback = pipeline[index];
            bool isLast = (index == pipeline.Count - 1);

            if (isLast)
            {
                await callback(request, response, null);
            }
            else
            {
                await callback(request, response, async () =>
                {
                    await ExecuteCallbackChain(index + 1, pipeline, request, response);
                });
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Get the route list for a specific HTTP method
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        private RouteDict GetRouteDict(string method)
        {
            return method.ToUpperInvariant() switch
            {
                "GET" => _getRoutes,
                "POST" => _postRoutes,
                "PUT" => _putRoutes,
                "DELETE" => _deleteRoutes,
                "PATCH" => _patchRoutes,
                "UPDATE" => _updateRoutes,
                _ => throw new ArgumentException($"Unsupported HTTP method: {method}"),
            };
        }

        /// <summary>
        /// Create a Route instance from method, path, and callbacks
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        private static Route CreateRoute(string method, string path, object[] callbacks)
        {
            Route route = new Route(method, path);

            foreach (var callback in callbacks)
            {
                // do not need to use conversions here since Route.Use method provides overloads
                // these overloads already do the conversions
                if (callback is RouteCallback routeCallback)
                {
                    route.Use(routeCallback);
                }
                else if (callback is MiddlewareCallback middlewareCallback)
                {
                    route.Use(middlewareCallback);
                }
                else if (callback is RequestHandler requestHandler)
                {
                    route.Use(requestHandler);
                }
                else
                {
                    throw new ArgumentException($"Invalid callback type: {callback.GetType().Name}");
                }
            }

            return route;
        }

        #endregion
    }
}