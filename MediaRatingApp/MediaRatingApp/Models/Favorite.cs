namespace MediaRatingApp.Models
{
    public class Favorite
    {
        public int UserId { get; set; }
        public int MediaId { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public User? User { get; set; }
        public Media? Media { get; set; }
    }
}
