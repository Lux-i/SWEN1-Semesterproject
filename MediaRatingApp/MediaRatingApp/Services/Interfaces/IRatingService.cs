using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaRatingApp.Models;

namespace MediaRatingApp.Services.Interfaces
{
    interface IRatingService
    {
        Task<int> CreateRatingAsync(int mediaId, int userId, int score, string? comment);
        Task UpdateRatingAsync(int ratingId, int userId, int score, string? comment);
        Task DeleteRatingAsync(int ratingId, int userId);
        Task<List<Rating>> GetRatingsByMediaIdAsync(int mediaId);
        Task<Rating?> GetRatingByIdAsync(int ratingId);
        Task ConfirmReviewAsync(int ratingId, int userId);
        Task<bool> ChangeLikeStatusAsync(int ratingId, int userId);
    }
}
