using MediaRatingApp.Models;

namespace MediaRatingApp.Data.Interfaces
{
    public interface IFavoriteRepository
    {
        Task<bool> AddFavoriteAsync(int userId, int mediaId);

        Task<bool> RemoveFavoriteAsync(int userId, int mediaId);

        Task<bool> IsFavoriteAsync(int userId, int mediaId);

        Task<List<Media>> GetUserFavoritesAsync(int userId);

        Task<bool> AddToWatchlistAsync(int userId, int mediaId);

        Task<bool> RemoveFromWatchlistAsync(int userId, int mediaId);

        Task<bool> IsInWatchlistAsync(int userId, int mediaId);

        Task<List<Media>> GetUserWatchlistAsync(int userId);
    }
}
