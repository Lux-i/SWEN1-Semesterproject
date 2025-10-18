using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using WebServer.Routing.Interfaces;
using WebServer.Routing.Models;

namespace WebServer.Routing
{
    class Routing
    {
        //utility class to setup routes to be passed to the router
        private RouteDictCollection routes;

        public Routing() { }
        
        public void Get(IRoute route)
        {
            routes.GetRoutes.Add(route.Path, route.Callback);
        }

        public void Post(IRoute route)
        {
            routes.PostRoutes.Add(route.Path, route.Callback);
        }

        public void Put(IRoute route)
        {
            routes.PutRoutes.Add(route.Path, route.Callback);
        }

        public void Delete(IRoute route)
        {
            routes.DeleteRoutes.Add(route.Path, route.Callback);
        }

        public void Update(IRoute route)
        {
            routes.UpdateRoutes.Add(route.Path, route.Callback);
            this.Get(new Route("/test", (HttpRequest req, HttpResponse res) =>
            {
                res.Send("Hello World from GET /test");
                return Task.CompletedTask;
            }));

            //create a callback function with the signature postCallback of type RouteCallback
            //then pass this function into this.Post()
            RouteCallback postCallback = (HttpRequest req, HttpResponse res) =>
            {
                res.Send("Hello World from POST /test");
                return Task.CompletedTask;
            };
            this.Post(new Route("/post-test", postCallback));
        }
    }
}
