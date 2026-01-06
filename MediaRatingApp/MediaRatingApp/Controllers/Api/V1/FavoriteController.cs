using MediaRatingApp.Controllers.Models;
using MediaRatingApp.Services.Implementations;
using WebServer.Models;

namespace MediaRatingApp.Controllers.Api.V1
{
    internal class FavoriteController : Controller
    {
        private readonly FavoriteService _favoriteService;

        public FavoriteController(FavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
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
                            int userId = req.CustomData.UserId;
                            var favorites = await _favoriteService.GetUserFavoritesAsync(userId);
                            res.SendJson(favorites);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Unknow error: {ex.Message}");
                            res.Status(500).SendJson(new { error = "Internal server error" });
                        }
                    }
                )
            );

            Router.Post(
                "/:mediaId",
                (RouteCallback)(
                    async (req, res) =>
                    {
                        try
                        {
                            if (!int.TryParse(req.GetPathParam("mediaId"), out int mediaId))
                            {
                                res.Status(400).SendJson(new { error = "Invalid 'mediaId'" });
                                return;
                            }

                            int userId = req.CustomData.UserId;
                            bool added = await _favoriteService.AddFavoriteAsync(userId, mediaId);
                            if (added)
                            {
                                res.Status(201).SendJson(new { success = true });
                            }
                            else
                            {
                                res.Status(400).SendJson(new { error = "Failed to add favorite" });
                            }
                        }
                        catch
                        {
                            res.Status(500).SendJson(new { error = "Internal server error" });
                        }
                    }
                )
            );

            Router.Delete(
                "/:mediaId",
                (RouteCallback)(
                    async (req, res) =>
                    {
                        try
                        {
                            if (!int.TryParse(req.GetPathParam("mediaId"), out int mediaId))
                            {
                                res.Status(400).SendJson(new { error = "Invalid 'mediaId'" });
                                return;
                            }

                            int userId = req.CustomData.UserId;
                            bool removed = await _favoriteService.RemoveFavoriteAsync(
                                userId,
                                mediaId
                            );
                            if (removed)
                            {
                                res.Status(204).Send();
                            }
                            else
                            {
                                res.Status(404).SendJson(new { error = "Favorite not found" });
                            }
                        }
                        catch
                        {
                            res.Status(500).SendJson(new { error = "Internal server error" });
                        }
                    }
                )
            );

            Router.Get(
                "/:mediaId/exists",
                (RouteCallback)(
                    async (req, res) =>
                    {
                        try
                        {
                            if (!int.TryParse(req.GetPathParam("mediaId"), out int mediaId))
                            {
                                res.Status(400).SendJson(new { error = "Invalid 'mediaId'" });
                                return;
                            }

                            int userId = req.CustomData.UserId;
                            bool exists = await _favoriteService.IsFavoriteAsync(userId, mediaId);
                            res.SendJson(new { isFavorite = exists });
                        }
                        catch
                        {
                            res.Status(500).SendJson(new { error = "Internal server error" });
                        }
                    }
                )
            );
        }
    }
}
