using System;
using System.Threading.Tasks;
using WebServer;
using WebServer.Models;
using WebServer.Routing;
using WebServer.Routing.Models;

namespace MediaRatingApp.WebServer
{
    // Just a simple example/test server listening on http://localhost/ and http://localhost:8080/
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Router router = new Router();

            router.Get("/", (RouteCallback)(async (req, res) =>
                {
                    res.SendHtml("<html><body><h1>Test HTML Page</h1><h2>WebServer</h2></body></html>");
                    await Task.CompletedTask;
                }));
            router.Get("/api/hello", (RouteCallback)(async (req, res) =>
            {
                res.SendJson(new { message = "Hello, World!" });
                await Task.CompletedTask;
            }));
            router.Get("/api/user/:userId", (RouteCallback)(async (req, res) =>
            {
                // Access path parameters
                string userId = req.GetPathParam("userId");
                res.SendJson(new { userId, name = "John Doe" });
                await Task.CompletedTask;
            }));
            router.Get("/api/search", (RouteCallback)(async (req, res) =>
            {
                // Access query parameters
                string query = req.GetQueryParam("q");
                string page = req.GetQueryParam("page");
                string[] results = ["result1", "result2"];
                res.SendJson(new { query, page, results });
                await Task.CompletedTask;
            }));
            router.Post("/api/user", (RouteCallback)(async (req, res) =>
            {
                string body = req.Body;

                res.Status(201).SendJson(new { success = true, data = body });
                await Task.CompletedTask;
            }));
            router.Put("/api/user/:userId", (RouteCallback)(async (req, res) =>
            {
                string userId = req.GetPathParam("userId");
                string body = req.Body;
                res.SendJson(new { updated = true, userId });
                await Task.CompletedTask;
            }));
            router.Delete("/api/user/:userId", (RouteCallback)(async (req, res) =>
            {
                string userId = req.GetPathParam("userId");
                res.Status(204).Send();
                await Task.CompletedTask;
            }));

            router.Get("/api/chaining", (RouteCallback)(async (req, res) =>
            {
                res.Status(200)
                   .Type("application/json")
                   .Header("X-Custom-Header", "CustomValue")
                   .Send("{\"chained\": true}");
                await Task.CompletedTask;
            }));

            router.Get("/api/middleware", (MiddlewareCallback)(async (req, res, next) =>
            {
                Console.WriteLine($"Received request from {req.IpAddress}");
                req.CustomData.MiddlewareTimestamp = DateTime.UtcNow;
                await next();
            }), (RouteCallback)(async (req, res) =>
            {
                res.SendJson(new
                {
                    message = "Middleware executed",
                    middlewareTimestamp = req.CustomData.MiddlewareTimestamp
                });
                await Task.CompletedTask;
            }));

            router.CatchAll(async (req, res) =>
            {
                res.Status(404).SendJson(new
                {
                    error = "Not Found",
                    path = req.Path,
                    method = req.Method.Method
                });
                await Task.CompletedTask;
            });

            Router subRouter = new Router();

            // Middleware running only on subRouter scope
            MiddlewareCallback subRouterMiddleware = async (req, res, next) =>
            {
                Console.WriteLine("Subrouter middleware executed");
                req.CustomData.MiddlewareMessage = "Subrouter middleware executed";
                await next();
            };

            subRouter.Use(subRouterMiddleware);

            subRouter.Get("/subroute", (RouteCallback)(async (req, res) =>
            {
                res.SendJson(new { message = "Subroute reached", middlewareMessage = req.CustomData.MiddlewareMessage });
                await Task.CompletedTask;
            }));

            router.Use("/subrouter", subRouter);

            var server = new Server(["http://localhost/", "http://localhost:8080/"], router);

            Console.WriteLine("Starting server...");
            await server.Start();
        }
    }
}