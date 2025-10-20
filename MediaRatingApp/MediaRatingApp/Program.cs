using MediaRatingApp.Data.Repositories;
using MediaRatingApp.Services.Implementations;
using MediaRatingApp.Middleware;
using WebServer;
using WebServer.Models;
using WebServer.Routing;
using MediaRatingApp.Models;

namespace MediaRatingApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            #region Repos
            FakeUserRepository userRepo = new FakeUserRepository();
            FakeMediaRepository mediaRepo = new FakeMediaRepository();
            #endregion

            #region Services
            AuthenticationService authService = new AuthenticationService(userRepo);
            MediaService mediaService = new MediaService(mediaRepo);
            #endregion

            Router router = new Router();

            //login and register routes
            router.Post("/register", (RouteCallback)(async (req, res) =>
            {
                var body = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(req.Body);
                var user = await authService.RegisterAsync(body["username"], body["password"], body["email"]);
                res.SendJson(new { userId = user._Id, username = user.Username, email = user.Email });
            }));

            router.Post("/login", (RouteCallback)(async (req, res) =>
            {
                var body = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(req.Body);
                var token = await authService.LoginAsync(body["username"], body["password"]);
                if (token == null)
                {
                    res.Status(401).SendJson(new { error = "Invalid credentials" });
                    return;
                }
                else
                {
                    res.SendJson(new { token });
                }
            }));

            Router apiRouter = new Router();

            var authMw = AuthMiddleware.Create(authService);

            apiRouter.Use(authMw);

            apiRouter.Get("/media", (RouteCallback)(async (req, res) =>
            {
                var media = await mediaService.GetAllAsync();
                res.SendJson(media);
            }));

            apiRouter.Get("/media/:id", (RouteCallback)(async (req, res) =>
            {
                int id = int.Parse(req.GetPathParam("id"));
                var media = await mediaService.GetByIdAsync(id);
                if (media == null)
                {
                    res.Status(404).SendJson(new { error = "Not found" });
                    return;
                }
                res.SendJson(media);
            }));

            apiRouter.Post("/media", (RouteCallback)(async (req, res) =>
            {
                int userId = req.CustomData.UserId;
                var media = System.Text.Json.JsonSerializer.Deserialize<Media>(req.Body);
                var created = await mediaService.CreateAsync(media, userId);
                res.Status(201).SendJson(created);
            }));

            apiRouter.Put("/media/:id", (RouteCallback)(async (req, res) =>
            {
                int id = int.Parse(req.GetPathParam("id"));
                int userId = req.CustomData.UserId;
                var media = System.Text.Json.JsonSerializer.Deserialize<Media>(req.Body);
                bool updated = await mediaService.UpdateAsync(id, media, userId);
                if (!updated)
                {
                    res.Status(403).SendJson(new { error = "Forbidden or not found" });
                    return;
                }
                res.SendJson(new { success = true });
            }));

            apiRouter.Delete("/media/:id", (RouteCallback)(async (req, res) =>
            {
                int id = int.Parse(req.GetPathParam("id"));
                int userId = req.CustomData.UserId;
                bool deleted = await mediaService.DeleteAsync(id, userId);
                if (!deleted)
                {
                    res.Status(403).SendJson(new { error = "Forbidden or not found" });
                    return;
                }
                res.Status(204).Send();
            }));

            router.Use("/api", apiRouter);

            Server server = new Server(["http://localhost/"], router);
            server.Start();
        }
    }
}
