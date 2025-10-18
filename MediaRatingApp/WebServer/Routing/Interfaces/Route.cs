using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebServer.Routing.Models;

namespace WebServer.Routing.Interfaces
{
    interface IRoute
    {
        string Path { get; }
        RouteCallback Callback { get; }
    }
}
