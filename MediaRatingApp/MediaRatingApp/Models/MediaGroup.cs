namespace MediaRatingApp.Models
{
    public class MediaGroup
    {
        public int Id { get; set; }
        public string GroupName { get; set; } = null!;
        public string GroupType { get; set; } = null!;

        public List<Media>? Media = new List<Media>();

        public List<MediaGroup> Children = new();
        public List<MediaGroup> Parents = new();
    }
}
