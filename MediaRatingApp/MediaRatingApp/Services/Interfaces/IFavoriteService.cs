using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaRatingApp.Models;

namespace MediaRatingApp.Services.Interfaces
{
    interface IFavoriteService
    {
        Task<bool> ChangeFavoriteStatusAsync(int mediaId, int userId);
        Task<List<Media>> GetUserFavoritesAsync(int userId);
    }
}
