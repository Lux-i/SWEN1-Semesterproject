namespace MediaRatingApp.Models
{
    public class Episode
    {
        public int Id { get; set; }
        public int SeasonId { get; set; }

        public int EpisodeNumber { get; set; }
        public string? Title { get; set; }

        public int? DurationMinutes { get; set; }
        public DateOnly? ReleaseDate { get; set; }

        public Season? Season { get; set; }
    }
}
