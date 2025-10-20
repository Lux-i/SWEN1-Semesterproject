using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaRatingApp.Models;

namespace MediaRatingApp.Services.Interfaces
{
    interface IMediaService
    {
        Task<Media> CreateAsync(Media media, int userId);
        Task UpdateAsync(int mediaId, Media updatedMedia, int userId);
        Task DeleteAsync(int mediaId, int userId);
        Task<Media> GetByIdAsync(int mediaId);
        Task<List<Media>> SearchAsync(string query, int page, int pageSize);
        Task<List<Media>> GetRecommendationsAsync(int userId);
    }
}
