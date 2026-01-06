using MediaRatingApp.Data.Interfaces;
using MediaRatingApp.Data.Repositories;
using MediaRatingApp.Models;
using MediaRatingApp.Services.Interfaces;

namespace MediaRatingApp.Services.Implementations
{
    class RatingService : IRatingService
    {
        private readonly IRatingRepository _ratingRepo;
        private readonly IMediaRepository _mediaRepo;

        public RatingService(IRatingRepository ratingRepo, IMediaRepository mediaRepo)
        {
            _ratingRepo = ratingRepo;
            _mediaRepo = mediaRepo;
        }

        public async Task<int> CreateRatingAsync(int mediaId, int userId, int stars, string? review)
        {
            Rating? existing = await _ratingRepo.GetByUserAndMediaAsync(userId, mediaId);
            if (existing != null)
            {
                throw new InvalidOperationException(
                    "You have already rated this entry. Update it instead."
                );
            }

            Media? mediaExisting = await _mediaRepo.GetByIdAsync(mediaId);
            if (mediaExisting == null)
            {
                throw new KeyNotFoundException("Media entry not found.");
            }

            Rating newRating = new Rating
            {
                UserId = userId,
                MediaId = mediaId,
                Stars = stars,
                Review = review,
            };

            return await _ratingRepo.CreateAsync(newRating);
        }

        public async Task UpdateRatingAsync(int ratingId, int userId, int score, string? comment)
        {
            var rating = await _ratingRepo.GetByIdAsync(ratingId);
            if (rating == null)
                throw new KeyNotFoundException("Rating not found.");

            if (rating.UserId != userId)
                throw new UnauthorizedAccessException("User cannot edit this rating.");

            rating.Stars = score;
            rating.Review = comment;

            var updated = await _ratingRepo.UpdateAsync(rating);
            if (!updated)
                throw new InvalidOperationException("Failed to update rating.");
        }

        public async Task DeleteRatingAsync(int ratingId, int userId)
        {
            var rating = await _ratingRepo.GetByIdAsync(ratingId);
            if (rating == null)
                throw new KeyNotFoundException("Rating not found.");

            if (rating.UserId != userId)
                throw new UnauthorizedAccessException("User cannot delete this rating.");

            var deleted = await _ratingRepo.DeleteAsync(ratingId);
            if (!deleted)
                throw new InvalidOperationException("Failed to delete rating.");
        }

        public async Task<List<Rating>> GetRatingsByMediaIdAsync(int mediaId)
        {
            return await _ratingRepo.GetByMediaAsync(mediaId);
        }

        public async Task<Rating?> GetRatingByIdAsync(int ratingId)
        {
            return await _ratingRepo.GetByIdAsync(ratingId);
        }

        public async Task ConfirmReviewAsync(int ratingId, int userId)
        {
            var rating = await _ratingRepo.GetByIdAsync(ratingId);
            if (rating == null)
                throw new KeyNotFoundException("Rating not found.");

            // Adjust this rule if admins/moderators should confirm instead
            if (rating.UserId != userId)
                throw new UnauthorizedAccessException("User cannot confirm this rating.");

            var updated = await _ratingRepo.UpdateConfirmedAsync(ratingId, true);
            if (!updated)
                throw new InvalidOperationException("Failed to confirm rating.");
        }

        public async Task<bool> ChangeLikeStatusAsync(int ratingId, int userId)
        {
            var rating = await _ratingRepo.GetByIdAsync(ratingId);
            if (rating == null)
                throw new KeyNotFoundException("Rating not found.");
            if (rating.UserId == userId)
                throw new UnauthorizedAccessException("You cannot like you own rating");

            var hasLiked = await _ratingRepo.HasUserLikedAsync(userId, ratingId);

            if (hasLiked)
            {
                await _ratingRepo.UnlikeAsync(userId, ratingId);
                return false;
            }
            else
            {
                await _ratingRepo.LikeAsync(userId, ratingId);
                return true;
            }
        }
    }
}
