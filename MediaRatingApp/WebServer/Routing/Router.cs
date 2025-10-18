using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebServer.Routing.Models;

namespace WebServer.Routing
{
    class Router
    {
        //router gets requested path and checks against registered routes
        public Router(RouteDictCollection routes)
        {
            //initialize router with routes from Routing class
        }

        public void Route(HttpMethod method, string path)
        {
            //check if requested path has a route registered under used method

            //check using dictionary function

            //if there was no direct match:
            //create a collection of all routes,
            //do regex matching for each route,
            //create new collection with remaining matched routes,
            //repeat until only one route remains or no routes remain
            //in the edge case that 2 or more routes remain, choose the first one

            //if a route is found, return it's RouteCallback
        }

        //public RouteStatus Route(HttpMethod method, string path) { }

        public void RegisterRoute(Route route) { }

        public void RegisterRoutes(List<Route> routes) { }

        public void UnregisterRoute(string routePath) { }

        public void UnregisterRoutes(List<string> routePaths) { }

        /// <summary>
        /// Registers a callback that will be executed if no route is found.
        /// This should be used instead of a '*' route,
        /// since the dictionaries do not guarantee order preservation
        /// </summary>
        /// <param name="callback"></param>
        public void CatchAll(RouteCallback callback) { }
    }
}
