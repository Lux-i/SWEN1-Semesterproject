using System.Text.Json;
using MediaRatingApp.Controllers.Models;
using MediaRatingApp.Middleware;
using MediaRatingApp.Services.Interfaces;
using WebServer.Models;
using WebServer.Routing;

namespace MediaRatingApp.Controllers
{
    internal class AuthController : Controller
    {
        private readonly IAuthenticationService authService;

        public AuthController(IAuthenticationService authService)
        {
            this.authService = authService;
        }

        public override void ConfigureRoutes()
        {
            //login and register routes
            Router.Post(
                "/register",
                (RouteCallback)(
                    async (req, res) =>
                    {
                        try
                        {
                            var body = JsonSerializer.Deserialize<Dictionary<string, string>>(
                                req.Body
                            );

                            // Following check could be turned into a middleware
                            if (
                                body == null
                                || !body.ContainsKey("username")
                                || !body.ContainsKey("password")
                            )
                            {
                                res.Status(400)
                                    .SendJson(new { error = "Request body missing data" });
                                return;
                            }

                            var user = await authService.RegisterAsync(
                                body["username"],
                                body["password"]
                            );
                            res.SendJson(new { userId = user.Id, username = user.Username }, 201);
                        }
                        catch (ArgumentException ex)
                        {
                            res.Status(400).SendJson(new { error = ex.Message });
                        }
                        catch (InvalidOperationException ex)
                        {
                            res.Status(409).SendJson(new { error = ex.Message });
                        }
                        catch
                        {
                            res.Status(500).SendJson(new { error = "Internal Server Error" });
                        }
                    }
                )
            );

            Router.Post(
                "/login",
                (RouteCallback)(
                    async (req, res) =>
                    {
                        try
                        {
                            var body = JsonSerializer.Deserialize<Dictionary<string, string>>(
                                req.Body
                            );

                            if (
                                body == null
                                || !body.ContainsKey("username")
                                || !body.ContainsKey("password")
                            )
                            {
                                res.Status(400)
                                    .SendJson(new { error = "Request body missing data" });
                                return;
                            }

                            var token = await authService.LoginAsync(
                                body["username"],
                                body["password"]
                            );
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
                    }
                )
            );

            Router.Post(
                "/logout",
                AuthMiddleware.IsAuth(authService),
                (RouteCallback)(
                    async (req, res) =>
                    {
                        try
                        {
                            int userId = req.CustomData.UserId;
                            bool result = authService.LogoutUser(userId);
                            if (result)
                            {
                                res.SendJson(new { message = "Logout successful" });
                            }
                            else
                            {
                                res.Status(400).SendJson(new { error = "Logout failed" });
                            }
                        }
                        catch
                        {
                            res.Status(500).SendJson(new { error = "Internal Server Error" });
                        }
                    }
                )
            );

            Router.Put(
                "/change-password",
                AuthMiddleware.IsAuth(authService),
                (RouteCallback)(
                    async (req, res) =>
                    {
                        try
                        {
                            var body = JsonSerializer.Deserialize<Dictionary<string, string>>(
                                req.Body
                            );
                            if (
                                body == null
                                || !body.ContainsKey("oldPassword")
                                || !body.ContainsKey("newPassword")
                            )
                            {
                                res.Status(400)
                                    .SendJson(new { error = "Request body missing data" });
                                return;
                            }
                            int userId = req.CustomData.UserId;
                            bool result = await authService.UpdateUserPassword(
                                userId,
                                body["oldPassword"],
                                body["newPassword"]
                            );
                            if (result)
                            {
                                res.SendJson(new { message = "Password changed successfully" });
                            }
                            else
                            {
                                res.Status(500)
                                    .SendJson(new { error = "Failed to update password" });
                            }
                        }
                        catch (InvalidOperationException)
                        {
                            res.Status(404)
                                .SendJson(new { error = "Missing User - Could not fetch" });
                        }
                        catch (UnauthorizedAccessException)
                        {
                            res.Status(401).SendJson(new { error = "Wrong Credentials" });
                        }
                        catch
                        {
                            res.Status(500).SendJson(new { error = "Internal Server Error" });
                        }
                    }
                )
            );

            Router.Delete(
                "/delete-account",
                AuthMiddleware.IsAuth(authService),
                (RouteCallback)(
                    async (req, res) =>
                    {
                        try
                        {
                            int userId = req.CustomData.UserId;
                            bool result = await authService.DeleteUserAsync(userId);
                            if (result)
                            {
                                res.SendJson(new { message = "Account deleted successfully" });
                            }
                            else
                            {
                                res.Status(500)
                                    .SendJson(new { error = "Failed to delete account" });
                            }
                        }
                        catch (InvalidOperationException)
                        {
                            res.Status(404)
                                .SendJson(new { error = "Missing User - Could not fetch" });
                        }
                        catch
                        {
                            res.Status(500).SendJson(new { error = "Internal Server Error" });
                        }
                    }
                )
            );
        }
    }
}
