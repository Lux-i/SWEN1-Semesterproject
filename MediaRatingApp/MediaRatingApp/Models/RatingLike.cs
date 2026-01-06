namespace MediaRatingApp.Models
{
    public class RatingLike
    {
        public int UserId { get; set; }
        public int RatingId { get; set; }

        public User? User;
        public Rating? Rating;
    }
}
