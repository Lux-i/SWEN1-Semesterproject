namespace MediaRatingApp.Models
{
    public class UserProfile
    {
        public int UserId { get; set; }
        public string? DisplayName { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public int TotalRatings { get; set; }

        public User? User;
    }

    // / A slim version of UserProfile for scenarios where only basic info is needed.
    public class UserProfileSlim
    {
        public int UserId { get; set; }
        public string? DisplayName { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
