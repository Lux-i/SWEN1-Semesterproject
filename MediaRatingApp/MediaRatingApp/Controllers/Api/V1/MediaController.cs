using System.Text.Json;
using MediaRatingApp.Controllers.Models;
using MediaRatingApp.Middleware;
using MediaRatingApp.Models;
using MediaRatingApp.Services.Interfaces;
using WebServer.Models;

namespace MediaRatingApp.Controllers.Api.V1
{
    class MediaController : Controller
    {
        private readonly IMediaService mediaService;

        public MediaController(IMediaService mediaService)
        {
            this.mediaService = mediaService;
        }

        public override void ConfigureRoutes()
        {
            Router.Get(
                "/",
                (RouteCallback)(
                    async (req, res) =>
                    {
                        try
                        {
                            var media = await mediaService.GetAllAsync();
                            var props = media.GetType().GetProperties();
                            res.SendJson(media);
                        }
                        catch
                        {
                            res.Status(500).SendJson(new { error = "Request error" });
                        }
                    }
                )
            );

            Router.Get(
                "/:id",
                (RouteCallback)(
                    async (req, res) =>
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
                                res.Status(404).Send();
                                return;
                            }

                            res.SendJson(media);
                            return;
                        }
                        catch
                        {
                            res.Status(500).SendJson(new { error = "Request error" });
                            return;
                        }
                    }
                )
            );

            Router.Post(
                "/",
                (RouteCallback)(
                    async (req, res) =>
                    {
                        try
                        {
                            int userId = req.CustomData.UserId;

                            MediaCreateDto? mediaCreationData =
                                JsonSerializer.Deserialize<MediaCreateDto>(req.Body);

                            if (mediaCreationData == null)
                            {
                                res.Status(400).SendJson(new { error = "Failed to parse media" });
                                return;
                            }

                            var created = await mediaService.CreateAsync(
                                mediaCreationData.ToMedia(),
                                userId
                            );
                            res.Status(201).SendJson(new { id = created });
                        }
                        catch (ArgumentException ex)
                        {
                            res.Status(400)
                                .SendJson(new { error = $"Invalid or missing data: {ex.Message}" });
                        }
                        catch (InvalidOperationException ex)
                        {
                            res.Status(409).SendJson(new { error = $"Conflict: {ex.Message}" });
                        }
                        catch
                        {
                            res.Status(500).SendJson(new { error = "Internal Server Error" });
                        }
                    }
                )
            );

            Router.Put(
                "/:id",
                OwnershipMiddleware.IsMediaOwner(mediaService, "id"),
                (RouteCallback)(
                    async (req, res) =>
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

                            Media? media = JsonSerializer.Deserialize<Media>(req.Body);

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
                            res.Status(204).Send();
                        }
                        catch (Exception ex)
                        {
                            res.Status(500)
                                .SendJson(new { error = $"Request error: {ex.Message}" });
                        }
                    }
                )
            );

            Router.Delete(
                "/:id",
                OwnershipMiddleware.IsMediaOwner(mediaService, "id"),
                (RouteCallback)(
                    async (req, res) =>
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
                        catch
                        {
                            res.Status(500).SendJson(new { error = "Request error" });
                        }
                    }
                )
            );
        }
    }
}
