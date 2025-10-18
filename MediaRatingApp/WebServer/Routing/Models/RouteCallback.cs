using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer.Routing.Models
{
    //The class RouteCallback is a shortcut for defining a callback function that takes an HttpRequest and HttpResponse as parameters.
    //It does not add any functionality apart from being a fixed Action with HttpRequest and HttpResponse parameters.
    // The class RouteCallback is a delegate type for defining a callback function that takes an HttpRequest and HttpResponse as parameters.
    public delegate Task RouteCallback(HttpRequest request, HttpResponse response);
}
