using MediaRatingApp.Controllers.Interfaces;
using MediaRatingApp.Controllers.Models;

namespace MediaRatingApp.Controllers.Api.V1
{
    class V1Controller : Controller, IApiVersionController
    {
        private readonly UserController _userController;
        private readonly MediaController _mediaController;
        private readonly RatingController _ratingController;
        private readonly FavoriteController _favoriteController;
        private readonly WatchlistController _watchlistController;

        public V1Controller(
            UserController userController,
            MediaController mediaController,
            RatingController ratingControler,
            FavoriteController favoriteController,
            WatchlistController watchlistController
        )
        {
            _userController = userController;
            _mediaController = mediaController;
            _ratingController = ratingControler;
            _favoriteController = favoriteController;
            _watchlistController = watchlistController;
        }

        public override void ConfigureSubRouters()
        {
            Router.Use("/users", _userController.BuildRouter());
            Router.Use("/media", _mediaController.BuildRouter());
            Router.Use("/ratings", _ratingController.BuildRouter());
            Router.Use("/favorites", _favoriteController.BuildRouter());
            Router.Use("/watchlist", _watchlistController.BuildRouter());
        }
    }
}
