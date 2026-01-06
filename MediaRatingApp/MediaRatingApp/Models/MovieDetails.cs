namespace MediaRatingApp.Models
{
    public class MovieDetails
    {
        public int MediaId { get; set; }
        public int DurationMinutes { get; set; }

        public Media? Media;
    }
}
