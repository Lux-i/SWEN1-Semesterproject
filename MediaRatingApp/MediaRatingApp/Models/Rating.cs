namespace MediaRatingApp.Models
{
    public class Rating
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public int MediaId { get; set; }

        public int Stars { get; set; }
        public string? Review { get; set; }
        public bool IsConfirmed { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public User? User;
        public Media? Media;

        public int LikeCount { get; set; }
    }
}
