using MediaRatingApp.Models.Enums;

namespace MediaRatingApp.Models
{
    public class Media
    {
        public int Id { get; set; }

        public int CreatorId { get; set; }
        public User? Creator;

        public string Title { get; set; } = string.Empty;
        public string? MediaDescription { get; set; }

        public MediaType MediaType { get; set; }

        public DateOnly? ReleaseDate { get; set; }
        public int? AgeRestriction { get; set; }

        public string? ArtworkUrl { get; set; }

        public decimal AvgRating { get; set; }
        public int RatingCount { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        public List<Rating> Ratings = new();
        public List<MediaGroup> Groups = new();
    }

    public class MediaCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string? MediaDescription { get; set; }
        public string MediaType { get; set; } = string.Empty;
        public DateOnly? ReleaseDate { get; set; }
        public int? AgeRestriction { get; set; }
        public string? ArtworkUrl { get; set; }

        public Media ToMedia()
        {
            return new Media
            {
                Title = this.Title,
                MediaDescription = this.MediaDescription,
                MediaType = Enum.Parse<MediaType>(this.MediaType, true),
                ReleaseDate = this.ReleaseDate,
                AgeRestriction = this.AgeRestriction,
                ArtworkUrl = this.ArtworkUrl,
            };
        }
    }
}
