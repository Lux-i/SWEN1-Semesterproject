using System.Text.Json;
using MediaRatingApp.Controllers.Models;
using MediaRatingApp.Services.Implementations;
using WebServer.Models;
using WebServer.Routing;

namespace MediaRatingApp.Controllers.Api.V1
{
    internal class WatchlistController : Controller
    {
        private readonly FavoriteService _favoriteService;

        public WatchlistController(FavoriteService favoriteService)
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
                            var watchlist = await _favoriteService.GetUserWatchlistAsync(userId);
                            res.SendJson(watchlist);
                        }
                        catch
                        {
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
                            bool added = await _favoriteService.AddToWatchlistAsync(
                                userId,
                                mediaId
                            );
                            if (added)
                                res.Status(201).SendJson(new { success = true });
                            else
                                res.Status(400)
                                    .SendJson(new { error = "Failed to add to watchlist" });
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
                            bool removed = await _favoriteService.RemoveFromWatchlistAsync(
                                userId,
                                mediaId
                            );
                            if (removed)
                                res.Status(204).Send();
                            else
                                res.Status(404).SendJson(new { error = "Media not in watchlist" });
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
                            bool exists = await _favoriteService.IsInWatchlistAsync(
                                userId,
                                mediaId
                            );
                            res.SendJson(new { isInWatchlist = exists });
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
