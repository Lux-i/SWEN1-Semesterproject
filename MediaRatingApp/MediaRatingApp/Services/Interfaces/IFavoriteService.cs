using MediaRatingApp.Models;

namespace MediaRatingApp.Services.Interfaces
{
    interface IFavoriteService
    {
        #region Favorites
        Task<bool> AddFavoriteAsync(int userId, int mediaId);
        Task<bool> RemoveFavoriteAsync(int userId, int mediaId);
        Task<bool> ChangeFavoriteStatusAsync(int mediaId, int userId);
        Task<bool> IsFavoriteAsync(int mediaId, int userId);
        Task<List<Media>> GetUserFavoritesAsync(int userId);
        #endregion

        #region Watchlist
        Task<bool> AddToWatchlistAsync(int userId, int mediaId);
        Task<bool> RemoveFromWatchlistAsync(int userId, int mediaId);
        Task<bool> ChangeWatchlistStatusAsync(int mediaId, int userId);
        Task<bool> IsInWatchlistAsync(int mediaId, int userId);
        Task<List<Media>> GetUserWatchlistAsync(int userId);
        #endregion
    }
}
