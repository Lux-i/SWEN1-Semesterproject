using System;
using System.Threading.Tasks;
using WebServer;
using WebServer.Routing;
using WebServer.Routing.Models;

namespace MediaRatingApp.WebServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var routing = new Routing();

            routing.Get("/", async (req, res) =>
                {
                    res.SendHtml("<html><body><h1>Welcome to MediaRatingApp</h1></body></html>");
                    await Task.CompletedTask;
                });
            routing.Get("/api/hello", async (req, res) =>
            {
                res.SendJson(new { message = "Hello, World!" });
                await Task.CompletedTask;
            });
            routing.Get("/api/user/:userId", async (req, res) =>
            {
                // Access path parameters
                string userId = req.GetPathParam("userId");
                res.SendJson(new { userId, name = "John Doe" });
                await Task.CompletedTask;
            });
            routing.Get("/api/search", async (req, res) =>
            {
                // Access query parameters
                string query = req.GetQueryParam("q");
                string page = req.GetQueryParam("page");
                res.SendJson(new { query, page, results = new[] { "result1", "result2" } });
                await Task.CompletedTask;
            });
            routing.Post("/api/user", async (req, res) =>
            {
                string body = req.Body;

                res.Status(201).SendJson(new { success = true, data = body });
                await Task.CompletedTask;
            });
            routing.Put("/api/user/:userId", async (req, res) =>
            {
                string userId = req.GetPathParam("userId");
                string body = req.Body;
                res.SendJson(new { updated = true, userId });
                await Task.CompletedTask;
            });
            routing.Delete("/api/user/:userId", async (req, res) =>
            {
                string userId = req.GetPathParam("userId");
                res.Status(204).Send();
                await Task.CompletedTask;
            });

            routing.Get("/api/chaining", async (req, res) =>
            {
                res.Status(200)
                   .Type("application/json")
                   .Header("X-Custom-Header", "CustomValue")
                   .Send("{\"chained\": true}");
                await Task.CompletedTask;
            });

            var router = new Router(routing.GetRoutes());

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

            var server = new Server(["http://localhost:80/"], router);

            Console.WriteLine("Starting server...");
            server.Start();
        }
    }
}