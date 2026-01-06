namespace MediaRatingApp.Models
{
    public class Season
    {
        public int Id { get; set; }
        public int SeriesId { get; set; }

        public int SeasonNumber { get; set; }
        public DateOnly? ReleaseDate { get; set; }

        public Media? Series;
        public List<Episode> Episodes = new List<Episode>();
    }
}
