using MediaRatingApp.Data.Interfaces;
using MediaRatingApp.Models;
using MediaRatingApp.Services.Interfaces;

namespace MediaRatingApp.Services.Implementations
{
    class FavoriteService : IFavoriteService
    {
        private readonly IFavoriteRepository _favoriteRepo;

        public FavoriteService(IFavoriteRepository favoriteRepo)
        {
            _favoriteRepo = favoriteRepo;
        }

        #region Favorites
        public Task<bool> AddFavoriteAsync(int userId, int mediaId) =>
            _favoriteRepo.AddFavoriteAsync(userId, mediaId);

        public Task<bool> RemoveFavoriteAsync(int userId, int mediaId) =>
            _favoriteRepo.RemoveFavoriteAsync(userId, mediaId);

        public async Task<bool> ChangeFavoriteStatusAsync(int mediaId, int userId)
        {
            // Get current favorite status
            bool isFavorite = await _favoriteRepo.IsFavoriteAsync(userId, mediaId);
            if (isFavorite)
            {
                // If it's already a favorite, remove it
                return await _favoriteRepo.RemoveFavoriteAsync(userId, mediaId);
            }
            else
            {
                // If it's not a favorite, add it
                return await _favoriteRepo.AddFavoriteAsync(userId, mediaId);
            }
        }

        public Task<bool> IsFavoriteAsync(int userId, int mediaId) =>
            _favoriteRepo.IsFavoriteAsync(userId, mediaId);

        public Task<List<Media>> GetUserFavoritesAsync(int userId) =>
            _favoriteRepo.GetUserFavoritesAsync(userId);
        #endregion

        #region Watchlist
        public Task<bool> AddToWatchlistAsync(int userId, int mediaId) =>
            _favoriteRepo.AddToWatchlistAsync(userId, mediaId);

        public Task<bool> RemoveFromWatchlistAsync(int userId, int mediaId) =>
            _favoriteRepo.RemoveFromWatchlistAsync(userId, mediaId);

        public async Task<bool> ChangeWatchlistStatusAsync(int mediaId, int userId)
        {
            bool isInWatchlist = await _favoriteRepo.IsInWatchlistAsync(userId, mediaId);
            if (isInWatchlist)
                return await _favoriteRepo.RemoveFromWatchlistAsync(userId, mediaId);
            return await _favoriteRepo.AddToWatchlistAsync(userId, mediaId);
        }

        public Task<bool> IsInWatchlistAsync(int userId, int mediaId) =>
            _favoriteRepo.IsInWatchlistAsync(userId, mediaId);

        public Task<List<Media>> GetUserWatchlistAsync(int userId) =>
            _favoriteRepo.GetUserWatchlistAsync(userId);
        #endregion
    }
}
