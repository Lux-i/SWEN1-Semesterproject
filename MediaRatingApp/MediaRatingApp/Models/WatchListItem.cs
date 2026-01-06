namespace MediaRatingApp.Models
{
    public class WatchListItem
    {
        public int UserId { get; set; }
        public int MediaId { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public User? User { get; }
        public Media? Media { get; }
    }
}
