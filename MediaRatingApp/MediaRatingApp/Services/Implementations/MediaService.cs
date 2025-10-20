using MediaRatingApp.Data.Repositories;
using MediaRatingApp.Models;
using MediaRatingApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatingApp.Services.Implementations
{
    class MediaService /*: IMediaService*/
    {
        private readonly FakeMediaRepository _mediaRepo;

        public MediaService(FakeMediaRepository mediaRepo)
        {
            _mediaRepo = mediaRepo;
        }

        public async Task<Media> CreateAsync(Media media, int userId)
        {
            media.CreatedById = userId;
            media.CreatedAt = DateTime.UtcNow;
            int id = await _mediaRepo.CreateAsync(media);
            media._Id = id;
            return media;
        }

        public async Task<Media?> GetByIdAsync(int id)
        {
            return await _mediaRepo.GetByIdAsync(id);
        }

        public async Task<List<Media>> GetAllAsync()
        {
            return await _mediaRepo.GetAllAsync();
        }

        public async Task<bool> UpdateAsync(int mediaId, Media updated, int userId)
        {
            var existing = await _mediaRepo.GetByIdAsync(mediaId);
            if (existing == null || existing.CreatedById != userId)
                return false;

            updated._Id = mediaId;
            updated.CreatedById = userId;
            return await _mediaRepo.UpdateAsync(updated);
        }

        public async Task<bool> DeleteAsync(int mediaId, int userId)
        {
            var existing = await _mediaRepo.GetByIdAsync(mediaId);
            if (existing == null || existing.CreatedById != userId)
                return false;

            return await _mediaRepo.DeleteAsync(mediaId);
        }
    }
}
