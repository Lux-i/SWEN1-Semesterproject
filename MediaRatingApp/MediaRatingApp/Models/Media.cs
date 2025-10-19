using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using MediaRatingApp.Models.Enums;

namespace MediaRatingApp.Models
{
    abstract class Media
    {
        int _id;
        string Title;
        string Description;
        MediaType Type;
        int ReleaseYear;
        List<string> Genres;
        int AgeRating;
        int CreatedById;
        DateTime CreatedAt;
        DateTime? UpdatedAt;

        User Creator => GetCreator();
        List<Rating> Ratings => GetMediaRatings();
        double AverageRating => Ratings.Count > 0 ? Ratings.Average(r => r.Score) : 0.0;

        private User GetCreator()
        {
            return new User();
        }

        private List<Rating> GetMediaRatings()
        {
            return new List<Rating>();
        }
    }
}
