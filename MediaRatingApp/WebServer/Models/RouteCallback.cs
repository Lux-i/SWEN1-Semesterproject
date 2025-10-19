using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer.Models
{
    /// <summary>
    /// Delegate for Route endpoints
    /// </summary>
    public delegate Task RouteCallback(HttpRequest request, HttpResponse response);
}
