using MediaRatingApp.Data.Repositories;
using MediaRatingApp.Services.Implementations;
using WebServer;
using WebServer.Models;
using WebServer.Routing;

namespace MediaRatingApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            #region Repos
            UserRepository userRepo = new UserRepository();
            #endregion

            #region Services
            AuthenticationService Auth = new AuthenticationService(userRepo);
            #endregion

            Router router = new Router();

            //login and register routes
            router.Post("/register", (RouteCallback)(async (req, res) =>
            {
                res.SendJson(new { message = "Register endpoint" });
            }));

            router.Get("/login", (RouteCallback)(async (req, res) =>
            {
                res.SendJson(new { message = "Login endpoint" });
                await Task.CompletedTask;
            }));
        }
    }
}
