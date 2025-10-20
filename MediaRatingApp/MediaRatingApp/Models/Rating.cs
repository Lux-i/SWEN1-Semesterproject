using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatingApp.Models
{
    public class Rating
    {
        public int _Id { get; }
        public int MediaId { get; }
        public int UserId { get; }
        public int Score { get; }
        public string? Comment { get; }
        public bool IsConfirmed { get; }
        public DateTime CreatedAt { get; }
        public DateTime? UpdatedAt { get; }

        public Game Media => GetRatedGame();
        public User User => GetRatingUser();
        public List<RatingLike> Likes => GetRatingLikes();
        public int LikesCount => Likes.Count();

        private Game GetRatedGame()
        {
            return new Game();
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
