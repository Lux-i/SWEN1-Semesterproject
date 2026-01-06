using System.Text.Json;
using MediaRatingApp.Controllers.Models;
using MediaRatingApp.Models;
using MediaRatingApp.Services.Interfaces;
using WebServer.Models;

namespace MediaRatingApp.Controllers.Api.V1
{
    class UserController : Controller
    {
        private readonly IUserService userService;

        public UserController(IUserService userService)
        {
            this.userService = userService;
        }

        public override void ConfigureRoutes()
        {
            Router.Get(
                "/me",
                (RouteCallback)(
                    async (req, res) =>
                    {
                        try
                        {
                            UserOut user = await userService.GetUserFullByIdAsync(
                                req.CustomData.UserId
                            );
                            res.SendJson(user);
                        }
                        catch (Exception ex)
                        {
                            res.SetStatusCode(500)
                                .SendJson(new { error = $"Internal server error: {ex.Message}" });
                        }
                    }
                )
            );

            Router.Get(
                "/profiles",
                (RouteCallback)(
                    async (req, res) =>
                    {
                        try
                        {
                            List<UserProfileSlim> users =
                                await userService.GetAllUserProfilesAsync();
                            res.SendJson(users);
                        }
                        catch (Exception ex)
                        {
                            res.SetStatusCode(500)
                                .SendJson(new { error = $"Internal server error: {ex.Message}" });
                        }
                    }
                )
            );

            Router.Get(
                "/profile/:id",
                (RouteCallback)(
                    async (req, res) =>
                    {
                        try
                        {
                            bool idParsed = int.TryParse(req.GetPathParam("id"), out int userId);
                            if (!idParsed)
                            {
                                res.SetStatusCode(400)
                                    .SendJson(new { error = "Invalid user ID format." });
                                return;
                            }

                            UserProfile profile = await userService.GetUserProfileByIdAsync(userId);

                            res.SendJson(profile);
                        }
                        catch (Exception ex)
                        {
                            res.SetStatusCode(500)
                                .SendJson(new { error = $"Internal server error: {ex.Message}" });
                        }
                    }
                )
            );

            Router.Patch(
                "/profile",
                (RouteCallback)(
                    async (req, res) =>
                    {
                        try
                        {
                            UserProfile? userProfile = JsonSerializer.Deserialize<UserProfile>(
                                req.Body
                            );
                            if (userProfile == null)
                            {
                                res.SetStatusCode(400)
                                    .SendJson(new { error = "Invalid request body." });
                                return;
                            }

                            await userService.UpdateUserProfileAsync(
                                req.CustomData.UserId,
                                userProfile
                            );

                            res.SetStatusCode(204).Send();
                        }
                        catch
                        {
                            res.SetStatusCode(500)
                                .SendJson(new { error = "Internal server error." });
                        }
                    }
                )
            );
        }
    }
}
