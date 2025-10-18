using WebServer.Routing.Interfaces;
using WebServer.Models;

public class Route : IRoute
{
    public string Path { get; }
    public RouteCallback Callback { get; }

    public Route(string path, RouteCallback callback)
    {
        Path = path;
        Callback = callback;
    }
}
