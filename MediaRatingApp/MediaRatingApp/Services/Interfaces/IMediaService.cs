using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaRatingApp.Models;

namespace MediaRatingApp.Services.Interfaces
{
    public interface IMediaService
    {
        Task<int> CreateAsync(Media media, int userId);
        Task<Media?> GetByIdAsync(int id);
        Task<List<Media>> GetAllAsync();
        Task<bool> UpdateAsync(int mediaId, Media updatedMedia, int userId);
        Task<bool> DeleteAsync(int mediaId, int userId);
        Task<bool> IsOwner(int mediaId, int userId);
    }
}
