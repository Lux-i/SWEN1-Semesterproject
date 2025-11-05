using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebServer.Models;

namespace WebServer.Routing.Models
{
    class RouteDict : Dictionary<string, Route>
    {
        public RouteDict() : base()
        {
        }

        public RouteDict(IEnumerable<KeyValuePair<string, Route>> collection) : base(collection)
        {
        }
    }
}
