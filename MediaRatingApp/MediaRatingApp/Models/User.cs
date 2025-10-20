using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatingApp.Models
{
    public class User
    {
        public int _Id;
        public string Username { get; }
        public string PasswordHash { get; }
        public string Email { get; }
        public DateTime CreatedAt { get; }

        public List<Media> CreatedMedia => GetCreatedMedia();
        public List<Rating> Ratings => GetUserRatings();
        public List<Media> Favorites => GetFavorites();

        public User()
        {
            CreatedAt = DateTime.Now;
            Username = "Max";
            PasswordHash = "hashed_password";
            Email = "max.mustermann@gmail.com";
        }

        public User(string username, string passwordHash, string email)
        {
            Username = username;
            PasswordHash = passwordHash;
            Email = email;
            CreatedAt = DateTime.UtcNow;
        }

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
