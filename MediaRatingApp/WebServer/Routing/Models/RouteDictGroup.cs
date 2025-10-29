using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer.Routing.Models
{
    /// <summary>
    /// A grouping of static and dynamic route dictionaries.
    /// </summary>
    class RouteDictGroup
    {
        public RouteDict StaticRoutes;
        public RouteDict DynamicRoutes;
        public RouteDict AllRoutes { get => new RouteDict(StaticRoutes.Concat(DynamicRoutes)); }

        public RouteDictGroup()
        {
            StaticRoutes = new RouteDict();
            DynamicRoutes = new RouteDict();
        }
    }
}
