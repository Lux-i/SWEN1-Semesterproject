namespace MediaRatingApp.Models
{
    public class MediaGroupLink
    {
        public int GroupId { get; set; }
        public int MediaId { get; set; }

        public MediaGroup? Group;
        public Media? Media;
    }
}
