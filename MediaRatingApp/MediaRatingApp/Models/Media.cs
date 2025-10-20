using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using MediaRatingApp.Models.Enums;

namespace MediaRatingApp.Models
{
    public abstract class Media
    {
        public int _Id;
        public string Title;
        public string Description;
        public MediaType Type;
        public int ReleaseYear;
        public List<string> Genres;
        public int AgeRating;
        public int CreatedById;
        public DateTime CreatedAt;
        public DateTime? UpdatedAt;

        public User Creator => GetCreator();
        public List<Rating> Ratings => GetMediaRatings();
        public double AverageRating => Ratings.Count > 0 ? Ratings.Average(r => r.Score) : 0.0;

        public Media()
        {
            Genres = new List<string>();
            CreatedAt = DateTime.UtcNow;
            Title = "";
            Description = "";
        }

        private User GetCreator()
        {
            return new User("test", "test", "test");
        }

        private List<Rating> GetMediaRatings()
        {
            return new List<Rating>();
        }
    }
}
