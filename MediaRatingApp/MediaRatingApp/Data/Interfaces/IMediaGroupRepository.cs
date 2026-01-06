using MediaRatingApp.Models;

namespace MediaRatingApp.Data.Interfaces
{
    public interface IMediaGroupRepository
    {
        Task<MediaGroup?> GetByIdAsync(int id);

        Task<List<MediaGroup>> GetAllAsync();

        Task<int> CreateAsync(MediaGroup mediaGroup);

        Task<bool> UpdateAsync(MediaGroup mediaGroup);

        Task<bool> DeleteAsync(int id);

        Task<bool> LinkMediaAsync(int mediaId, int groupId);

        Task<bool> UnlinkMediaAsync(int mediaId, int groupId);

        Task<List<Media>> GetMediaByGroupAsync(int groupId);

        Task<List<MediaGroup>> GetGroupsByMediaAsync(int mediaId);
    }
}
