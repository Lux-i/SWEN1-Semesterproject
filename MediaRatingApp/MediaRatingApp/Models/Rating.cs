using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatingApp.Models
{
    class Rating
    {
        public int _Id { get; }
        public int MediaId { get; }
        public int UserId { get; }
        public int Score { get; }
        public string? Comment { get; }
        public bool IsConfirmed { get; }
        public DateTime CreatedAt { get; }
        public DateTime? UpdatedAt { get; }

        public Media Media => GetRatedMedia();
        public User User => GetRatingUser();
        public List<RatingLike> Likes => GetRatingLikes();
        public int LikesCount => Likes.Count(l => l.IsLike);

        private Media GetRatedMedia()
        {
            return new Media();
        }

        private User GetRatingUser()
        {
            return new User();
        }

        private List<RatingLike> GetRatingLikes()
        {
            return new List<RatingLike>();
        }
    }
}
