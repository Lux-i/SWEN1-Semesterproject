using System.Text.Json;
using MediaRatingApp.Controllers.Models;
using MediaRatingApp.Models;
using MediaRatingApp.Services.Interfaces;
using WebServer.Models;

namespace MediaRatingApp.Controllers.Api.V1
{
    internal class RatingController : Controller
    {
        private readonly IRatingService ratingService;

        public RatingController(IRatingService ratingService)
        {
            this.ratingService = ratingService;
        }

        public override void ConfigureRoutes()
        {
            Router.Get(
                "/media/:mediaId",
                (RouteCallback)(
                    async (req, res) =>
                    {
                        try
                        {
                            if (!int.TryParse(req.GetPathParam("mediaId"), out int mediaId))
                            {
                                res.SetStatusCode(400)
                                    .SendJson(new { error = "Invalid media ID format." });
                                return;
                            }

                            var ratings = await ratingService.GetRatingsByMediaIdAsync(mediaId);
                            res.SendJson(ratings);
                        }
                        catch
                        {
                            res.SetStatusCode(500)
                                .SendJson(new { error = $"Internal server error." });
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
                            if (!int.TryParse(req.GetPathParam("id"), out int ratingId))
                            {
                                res.SetStatusCode(400)
                                    .SendJson(new { error = "Invalid rating ID format." });
                                return;
                            }

                            var rating = await ratingService.GetRatingByIdAsync(ratingId);
                            if (rating == null)
                            {
                                res.SetStatusCode(404)
                                    .SendJson(new { error = "Rating not found." });
                                return;
                            }

                            res.SendJson(rating);
                        }
                        catch (Exception ex)
                        {
                            res.SetStatusCode(500)
                                .SendJson(new { error = $"Internal server error: {ex.Message}" });
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
                            var body = JsonSerializer.Deserialize<Rating>(req.Body);
                            if (body == null)
                            {
                                res.SetStatusCode(400)
                                    .SendJson(new { error = "Invalid request body." });
                                return;
                            }

                            int id = await ratingService.CreateRatingAsync(
                                body.MediaId,
                                req.CustomData.UserId,
                                body.Stars,
                                body.Review
                            );

                            res.SetStatusCode(201).SendJson(new { id });
                        }
                        catch (InvalidOperationException ex)
                        {
                            res.SetStatusCode(400).SendJson(new { error = ex.Message });
                        }
                        catch (KeyNotFoundException ex)
                        {
                            res.SetStatusCode(404).SendJson(new { error = ex.Message });
                        }
                        catch (Exception ex)
                        {
                            res.SetStatusCode(500)
                                .SendJson(new { error = $"Internal server error. {ex.Message}" });
                        }
                    }
                )
            );

            Router.Patch(
                "/:id",
                (RouteCallback)(
                    async (req, res) =>
                    {
                        try
                        {
                            if (!int.TryParse(req.GetPathParam("id"), out int ratingId))
                            {
                                res.SetStatusCode(400)
                                    .SendJson(new { error = "Invalid rating ID format." });
                                return;
                            }

                            var body = JsonSerializer.Deserialize<Rating>(req.Body);
                            if (body == null)
                            {
                                res.SetStatusCode(400)
                                    .SendJson(new { error = "Invalid request body." });
                                return;
                            }

                            await ratingService.UpdateRatingAsync(
                                ratingId,
                                req.CustomData.UserId,
                                body.Stars,
                                body.Review
                            );

                            res.SetStatusCode(204).Send();
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            res.SetStatusCode(403).SendJson(new { error = ex.Message });
                        }
                        catch (KeyNotFoundException ex)
                        {
                            res.SetStatusCode(404).SendJson(new { error = ex.Message });
                        }
                        catch
                        {
                            res.SetStatusCode(500)
                                .SendJson(new { error = $"Internal server error." });
                        }
                    }
                )
            );

            Router.Delete(
                "/:id",
                (RouteCallback)(
                    async (req, res) =>
                    {
                        try
                        {
                            if (!int.TryParse(req.GetPathParam("id"), out int ratingId))
                            {
                                res.SetStatusCode(400)
                                    .SendJson(new { error = "Invalid rating ID format." });
                                return;
                            }

                            await ratingService.DeleteRatingAsync(ratingId, req.CustomData.UserId);

                            res.SetStatusCode(204).Send();
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            res.SetStatusCode(403).SendJson(new { error = ex.Message });
                        }
                        catch (KeyNotFoundException ex)
                        {
                            res.SetStatusCode(404).SendJson(new { error = ex.Message });
                        }
                        catch
                        {
                            res.SetStatusCode(500)
                                .SendJson(new { error = $"Internal server error." });
                        }
                    }
                )
            );

            Router.Post(
                "/:id/confirm",
                (RouteCallback)(
                    async (req, res) =>
                    {
                        try
                        {
                            if (!int.TryParse(req.GetPathParam("id"), out int ratingId))
                            {
                                res.SetStatusCode(400)
                                    .SendJson(new { error = "Invalid rating ID format." });
                                return;
                            }

                            await ratingService.ConfirmReviewAsync(ratingId, req.CustomData.UserId);

                            res.SetStatusCode(204).Send();
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            res.SetStatusCode(403).SendJson(new { error = ex.Message });
                        }
                        catch (KeyNotFoundException ex)
                        {
                            res.SetStatusCode(404).SendJson(new { error = ex.Message });
                        }
                        catch
                        {
                            res.SetStatusCode(500)
                                .SendJson(new { error = $"Internal server error." });
                        }
                    }
                )
            );

            Router.Post(
                "/:id/like",
                (RouteCallback)(
                    async (req, res) =>
                    {
                        try
                        {
                            if (!int.TryParse(req.GetPathParam("id"), out int ratingId))
                            {
                                res.SetStatusCode(400)
                                    .SendJson(new { error = "Invalid rating ID format." });
                                return;
                            }

                            bool liked = await ratingService.ChangeLikeStatusAsync(
                                ratingId,
                                req.CustomData.UserId
                            );

                            res.SendJson(new { liked });
                        }
                        catch (KeyNotFoundException ex)
                        {
                            res.SetStatusCode(404).SendJson(new { error = ex.Message });
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            res.Status(401).SendJson(new { error = ex.Message });
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Unknown Error occured: {ex.Message}");
                            res.SetStatusCode(500)
                                .SendJson(new { error = $"Internal server error." });
                        }
                    }
                )
            );
        }
    }
}
