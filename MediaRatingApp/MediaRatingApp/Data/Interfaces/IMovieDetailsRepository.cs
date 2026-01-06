using MediaRatingApp.Models;

namespace MediaRatingApp.Data.Interfaces
{
    public interface IMovieDetailsRepository
    {
        Task<MovieDetails?> GetByMediaIdAsync(int mediaId);
        Task<bool> UpdateAsync(MovieDetails details);
    }
}
