using MediaRatingApp.Models;
using Npgsql;
using NpgsqlTypes;

namespace MediaRatingApp.Data.Interfaces
{
    public interface IMediaRepository
    {
        Task<Media?> GetByIdAsync(int id);

        Task<List<Media>> GetAllAsync();

        Task<int> CreateAsync(Media media);

        Task<bool> UpdateAsync(Media media);

        Task<bool> DeleteAsync(int id);

        Task<List<Media>> GetByCreatorAsync(int creatorId);

        Task<List<Media>> SearchByTitleAsync(string searchTerm, int limit = 50);

        Task<List<Media>> GetByGroupAsync(int groupId);

        /// <summary>
        /// Asynchronously retrieves a list of media items that match the specified filter criteria.
        /// Just a wrapper to enumerate <paramref name="filter"/> onto <see cref="NpgsqlParameterCollection.AddWithValue(string, object)"/>."/>
        /// </summary>
        /// <param name="filter"></param>
        /// <returns>A task which returns a List of all Media items that matched with the given filter/></returns>
        Task<List<Media>> GetWithFilterAsync(IEnumerable<KeyValuePair<string, object>> filter);
    }
}
