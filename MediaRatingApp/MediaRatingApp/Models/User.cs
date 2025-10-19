using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatingApp.Models
{
    class User
    {
        int _Id;
        string Username;
        string PasswordHash;
        string Email;
        DateTime CreatedAt;

        List<Media> CreatedMedia => GetCreatedMedia();
        List<Rating> Ratings => GetUserRatings();
        List<Media> Favorites => GetFavorites();

        private List<Media> GetCreatedMedia()
        {
            return new List<Media>();
        }

        private List<Rating> GetUserRatings()
        {
            return new List<Rating>();
        }

        private List<Media> GetFavorites()
        {
            return new List<Media>();
        }
    }
}
