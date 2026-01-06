using MediaRatingApp.Models;

namespace MediaRatingApp.Data.Interfaces
{
    public interface IRatingRepository
    {
        Task<Rating?> GetByIdAsync(int id);

        Task<Rating?> GetByUserAndMediaAsync(int userId, int mediaId);

        Task<List<Rating>> GetByMediaAsync(int mediaId);

        Task<List<Rating>> GetByUserAsync(int userId);

        Task<int> CreateAsync(Rating rating);

        Task<bool> UpdateAsync(Rating rating);

        Task<bool> UpdateConfirmedAsync(int ratingId, bool confirmStatus, int userId);

        Task<bool> DeleteAsync(int id);

        Task<bool> LikeAsync(int userId, int ratingId);

        Task<bool> UnlikeAsync(int userId, int ratingId);

        Task<bool> HasUserLikedAsync(int userId, int ratingId);

        Task<int> GetLikeCountAsync(int ratingId);
    }
}
