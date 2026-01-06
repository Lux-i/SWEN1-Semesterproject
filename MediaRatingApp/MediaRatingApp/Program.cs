using MediaRatingApp.Controllers;
using MediaRatingApp.Controllers.Interfaces;
using MediaRatingApp.Data.Repositories;
using MediaRatingApp.Models.Enums;
using MediaRatingApp.Services.Implementations;
using Npgsql;
using WebServer;
using WebServer.Routing;
using Api = MediaRatingApp.Controllers.Api;

namespace MediaRatingApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            #region Repos
            const string connectionString =
                "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=1234";
            NpgsqlDataSourceBuilder dataSourceBuilder = new NpgsqlDataSourceBuilder(
                connectionString
            );
            dataSourceBuilder.MapEnum<MediaType>("media_type");
            NpgsqlDataSource dataSource = dataSourceBuilder.Build();

            UserRepository userRepo = new UserRepository(dataSource);
            MediaRepository mediaRepo = new MediaRepository(dataSource);
            RatingRepository ratingRepo = new RatingRepository(dataSource);
            FavoriteRepository favoriteRepo = new FavoriteRepository(dataSource);
            #endregion

            #region Services
            AuthenticationService authService = new AuthenticationService(userRepo);
            MediaService mediaService = new MediaService(mediaRepo);
            UserService userService = new UserService(userRepo);
            RatingService ratingService = new RatingService(ratingRepo, mediaRepo);
            FavoriteService favoriteService = new FavoriteService(favoriteRepo);
            #endregion

            #region Controllers
            Api.V1.RatingController v1_ratingController = new Api.V1.RatingController(
                ratingService
            );
            Api.V1.FavoriteController v1_favoriteController = new Api.V1.FavoriteController(
                favoriteService
            );
            Api.V1.WatchlistController v1_watchlistController = new Api.V1.WatchlistController(
                favoriteService
            );
            Api.V1.MediaController v1_mediaController = new Api.V1.MediaController(mediaService);
            Api.V1.UserController v1_userController = new Api.V1.UserController(userService);

            #region Api Version Controllers
            Api.V1.V1Controller v1Controller = new Api.V1.V1Controller(
                v1_userController,
                v1_mediaController,
                v1_ratingController,
                v1_favoriteController,
                v1_watchlistController
            );

            // Api Version Controller Dictionary
            Dictionary<string, IApiVersionController> versionControllers = new Dictionary<
                string,
                IApiVersionController
            >
            {
                { "/v1", v1Controller },
            };
            #endregion

            Api.ApiController apiController = new Api.ApiController(
                versionControllers,
                authService
            );

            AuthController authController = new AuthController(authService);

            BaseController controller = new BaseController(apiController, authController);
            #endregion

            Router router = controller.BuildRouter();

            Server server = new Server(["http://localhost:1234/"], router);
            server.Start();

            while (true)
            {
                Console.Write("> ");
                string? input = Console.ReadLine();
                if (input != null)
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
