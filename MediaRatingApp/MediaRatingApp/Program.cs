using MediaRatingApp.Data.Repositories;
using MediaRatingApp.Services.Implementations;
using MediaRatingApp.Middleware;
using MediaRatingApp.Models.Enums;
using WebServer;
using WebServer.Models;
using WebServer.Routing;
using MediaRatingApp.Models;
using System.Text.Json;

namespace MediaRatingApp
{
    internal class Program
    {
        static async Task Main(string[] args)
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
                try
                {
                    var body = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(req.Body);

                    // Following check could be turned into a middleware
                    if (body == null || !body.ContainsKey("username") || !body.ContainsKey("password") || !body.ContainsKey("email"))
                    {
                        res.Status(400).SendJson(new { error = "Request body missing data" });
                        return;
                    }

                    var user = await authService.RegisterAsync(body["username"], body["password"], body["email"]);
                    res.SendJson(new { userId = user._Id, username = user.Username, email = user.Email });
                }
                catch
                {
                    res.Status(500).SendJson(new { error = "Request error" });
                }
            }));

            router.Post("/login", (RouteCallback)(async (req, res) =>
            {
                try
                {
                    var body = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(req.Body);

                    if (body == null || !body.ContainsKey("username") || !body.ContainsKey("password"))
                    {
                        res.Status(400).SendJson(new { error = "Request body missing data" });
                        return;
                    }

                    var token = await authService.LoginAsync(body["username"], body["password"]);
                    if (token == null)
                    {
                        res.Status(401).SendJson(new { error = "Invalid credentials" });
                        return;
                    }

                    res.SendJson(new { token });
                }
                catch
                {
                    res.Status(500).SendJson(new { error = "Request error" });
                }
            }));

            Router apiRouter = new Router();

            var authMw = AuthMiddleware.Create(authService);

            apiRouter.Use(authMw);

            apiRouter.Get("/media", (RouteCallback)(async (req, res) =>
            {
                try
                {
                    var media = await mediaService.GetAllAsync();
                    res.SendJson(media);
                }
                catch
                {
                    res.Status(500).SendJson(new { error = "Request error" });
                }
            }));

            apiRouter.Get("/media/:id", (RouteCallback)(async (req, res) =>
            {
                try
                {
                    if (!int.TryParse(req.GetPathParam("id"), out int id))
                    {
                        res.Status(400).SendJson(new { error = "Invalid 'id'" });
                        return;
                    }

                    var media = await mediaService.GetByIdAsync(id);
                    if (media == null)
                    {
                        res.Status(404).SendJson(new { error = "Not found" });
                        return;
                    }
                }
                catch
                {
                    res.Status(500).SendJson(new { error = "Request error" });
                    return;
                }
            }));

            apiRouter.Post("/media", (RouteCallback)(async (req, res) =>
            {
                try
                {
                    int userId = req.CustomData.UserId;

                    using var doc = JsonDocument.Parse(req.Body);
                    if (!doc.RootElement.TryGetProperty("Type", out var typeElement))
                    {
                        res.Status(400).SendJson(new { error = "Missing 'Type'" });
                        return;
                    }

                    MediaType mediaType = (MediaType)typeElement.GetInt32();
                    Media? media = null;

                    // Needs to be converted to a helper/util function
                    switch (mediaType)
                    {
                        case MediaType.Game:
                            media = JsonSerializer.Deserialize<Game>(req.Body);
                            break;
                        case MediaType.Movie:
                            media = JsonSerializer.Deserialize<Movie>(req.Body);
                            break;
                        case MediaType.Series:
                            media = JsonSerializer.Deserialize<Series>(req.Body);
                            break;
                        default:
                            res.Status(400).SendJson(new { error = "Invalid 'Type'" });
                            return;
                    }

                    if (media == null)
                    {
                        res.Status(400).SendJson(new { error = "Failed to parse media" });
                        return;
                    }

                    var created = await mediaService.CreateAsync(media, userId);
                    res.Status(201).SendJson(created);
                }
                catch
                {
                    res.Status(500).SendJson(new { error = "Request error" });
                }
            }));

            apiRouter.Put("/media/:id", (RouteCallback)(async (req, res) =>
            {
                try
                {
                    if (!int.TryParse(req.GetPathParam("id"), out int id))
                    {
                        res.Status(400).SendJson(new { error = "Invalid 'id'" });
                        return;
                    }

                    int userId = req.CustomData.UserId;

                    Media? storedMedia = await mediaService.GetByIdAsync(id);
                    if (storedMedia == null)
                    {
                        res.Status(404).SendJson(new { error = "Not found" });
                        return;
                    }

                    MediaType mediaType = storedMedia.Type;
                    Media? media = null;

                    switch (mediaType)
                    {
                        case MediaType.Game:
                            media = JsonSerializer.Deserialize<Game>(req.Body);
                            break;
                        case MediaType.Movie:
                            media = JsonSerializer.Deserialize<Movie>(req.Body);
                            break;
                        case MediaType.Series:
                            media = JsonSerializer.Deserialize<Series>(req.Body);
                            break;
                        default:
                            res.Status(400).SendJson(new { error = "Invalid 'Type'" });
                            return;
                    }

                    if (media == null)
                    {
                        res.Status(400).SendJson(new { error = "Failed to parse media" });
                        return;
                    }

                    bool updated = await mediaService.UpdateAsync(id, media, userId);
                    if (!updated)
                    {
                        res.Status(403).SendJson(new { error = "Forbidden or not found" });
                        return;
                    }
                    res.SendJson(new { success = true });
                }
                catch { res.Status(500).SendJson(new { error = "Request error" }); }
            }));

            apiRouter.Delete("/media/:id", (RouteCallback)(async (req, res) =>
            {
                try
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
                }
                catch { res.Status(500).SendJson(new { error = "Request error" }); }
            }));

            router.Use("/api", apiRouter);

            Server server = new Server(["http://localhost/"], router);
            server.Start();

            while (true) {
                Console.Write("> ");
                string? input = Console.ReadLine();
                if(input != null)
                {
                    switch (input)
                    {
                        case "stop":
                            server.Stop();
                            break;
                        case "start":
                            await server.Start();
                            break;
                        default:
                            Console.WriteLine("Unknown command");
                            break;
                    }
                }
            }
        }
    }
}
