using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer.Routing.Models
{
    class RouteDict : Dictionary<string, RouteCallback>
    {
        
    }

    struct RouteDictCollection
    {
        public RouteDict GetRoutes;
        public RouteDict PostRoutes;
        public RouteDict PutRoutes;
        public RouteDict DeleteRoutes;
        public RouteDict UpdateRoutes;
    }
}
