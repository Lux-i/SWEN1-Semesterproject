using MediaRatingApp.Services.Interfaces;
using WebServer.Models;

namespace MediaRatingApp.Middleware
{
    public static class AuthMiddleware
    {
        public static MiddlewareCallback IsAuth(IAuthenticationService authService)
        {
            return async (req, res, next) =>
            {
                string? token = req.GetHeader("Authorization")?.Replace("Bearer ", "");

                if (string.IsNullOrEmpty(token))
                {
                    res.Status(401).SendJson(new { error = "Unauthorized" });
                    return;
                }

                int? userId = authService.ValidateToken(token);
                if (userId == null)
                {
                    res.Status(401).SendJson(new { error = "Invalid token" });
                    return;
                }

                req.CustomData.UserId = userId.Value;
                await next();
            };
        }
    }
}
