using System.Text.Json;
using System.Text.Json.Serialization;
using MediaRatingApp.Data.Interfaces;
using MediaRatingApp.Models;
using MediaRatingApp.Models.Enums;
using MediaRatingApp.Services.Interfaces;

namespace MediaRatingApp.Services.Implementations
{
    public class MediaService : IMediaService
    {
        private readonly IMediaRepository _mediaRepo;

        public MediaService(IMediaRepository mediaRepo)
        {
            _mediaRepo = mediaRepo;
        }

        public async Task<int> CreateAsync(Media media, int userId)
        {
            // Validation check since DB side 'NOT NULL' does not prevent empty strings.
            if (String.IsNullOrEmpty(media.Title))
            {
                throw new ArgumentException("A media title must be provided");
            }

            if (await Exists(media.Title, media.MediaType, media.MediaDescription))
            {
                throw new InvalidOperationException("This media already has an entry.");
            }

            media.CreatorId = userId;

            return await _mediaRepo.CreateAsync(media);
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

            // Extra check in case middleware is not implemented, failed, or bypassed.
            if (existing == null)
            {
                throw new InvalidOperationException("Media entry does not exist.");
            }
            if (existing.CreatorId != userId)
            {
                throw new UnauthorizedAccessException(
                    "You do not have permission to update this media entry."
                );
            }

            // Merge new data into existing entry
            existing.Title = String.IsNullOrEmpty(updated.Title) ? existing.Title : updated.Title;
            existing.MediaDescription = updated.MediaDescription ?? existing.MediaDescription;
            existing.MediaType = updated.MediaType;
            existing.ReleaseDate = updated.ReleaseDate ?? existing.ReleaseDate;
            existing.AgeRestriction = updated.AgeRestriction ?? existing.AgeRestriction;
            existing.ArtworkUrl = updated.ArtworkUrl ?? existing.ArtworkUrl;

            return await _mediaRepo.UpdateAsync(existing);
        }

        public async Task<bool> DeleteAsync(int mediaId, int userId)
        {
            var existing = await _mediaRepo.GetByIdAsync(mediaId);

            // Extra check in case middleware is not implemented, failed, or bypassed.
            if (existing == null)
            {
                throw new InvalidOperationException("Media entry does not exist.");
            }
            if (existing.CreatorId != userId)
            {
                throw new UnauthorizedAccessException(
                    "You do not have permission to update this media entry."
                );
            }

            return await _mediaRepo.DeleteAsync(mediaId);
        }

        public async Task<bool> IsOwner(int mediaId, int userId)
        {
            var media = await _mediaRepo.GetByIdAsync(mediaId);

            if (media == null)
            {
                throw new InvalidOperationException("Media entry does not exist.");
            }

            return media.CreatorId == userId;
        }

        #region Helper Methods

        /// <summary>
        /// Checks if a media entry already exists with the same title, type, and optional description.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="mediaType"></param>
        /// <param name="mediaDescription"></param>
        /// <returns></returns>
        private async Task<bool> Exists(string title, MediaType mediaType, string? mediaDescription)
        {
            KeyValuePair<string, object>[] filter =
            {
                new KeyValuePair<string, object>("title", title),
                new KeyValuePair<string, object>("media_type", mediaType),
            };

            if (!String.IsNullOrEmpty(mediaDescription))
            {
                filter.Append(
                    new KeyValuePair<string, object>("media_description", mediaDescription)
                );
            }

            List<Media> existingMedia = await _mediaRepo.GetWithFilterAsync(filter);

            return existingMedia.Count > 0;
        }

        /// <summary>
        /// Checks if a media entry exists by its ID.
        /// Might be useful if a function needs to know if a certain id is a valid entry but does not want the Media object.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<bool> Exists(int id)
        {
            var media = await _mediaRepo.GetByIdAsync(id);
            return media != null;
        }

        #endregion
    }
}
