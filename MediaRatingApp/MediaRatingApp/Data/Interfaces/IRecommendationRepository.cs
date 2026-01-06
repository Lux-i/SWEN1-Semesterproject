using MediaRatingApp.Models;

namespace MediaRatingApp.Data.Interfaces
{
    public interface IRecommendationRepository
    {
        Task<IReadOnlyList<Media>> GetRecommendationsAsync(int userId, int limit = 20, int? excludeMediaId = null);

        Task<IReadOnlyList<MediaGroup>> GetSimilarMediaAsync(int mediaId, int limit = 20);
    }
}
