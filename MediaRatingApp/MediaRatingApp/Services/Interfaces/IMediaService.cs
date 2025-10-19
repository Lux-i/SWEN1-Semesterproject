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
        Task<Media> CreateMediaAsync(Media media, int userId);
        Task UpdateMediaAsync(int mediaId, Media updatedMedia, int userId);
        Task DeleteMediaAsync(int mediaId, int userId);
        Task<Media> GetMediaByIdAsync(int mediaId);
        Task<List<Media>> SearchMediaAsync(string query, int page, int pageSize);
        Task<List<Media>> GetRecommendationsAsync(int userId);
    }
}
