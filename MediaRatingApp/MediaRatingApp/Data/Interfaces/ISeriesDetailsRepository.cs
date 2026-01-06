using MediaRatingApp.Models;

namespace MediaRatingApp.Data.Interfaces
{
    public interface ISeriesDetailsRepository
    {
        Task<List<Season>> GetSeasonsAsync(int seriesMediaId);
        Task<List<Episode>> GetEpisodesAsync(int seriesMediaId);
        Task<bool> UpdateSeasonAsync(Season season);
        Task<bool> UpdateEpisodeAsync(Episode episode);
    }
}
