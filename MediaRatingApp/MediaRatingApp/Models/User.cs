using System.Text.Json.Serialization;

namespace MediaRatingApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;

        [JsonIgnore] // Exclude PwHash from JSON serialization
        public string PwHash { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
        public UserProfile? Profile { get; set; }
    }

    // User model without PwHash for outgoing data
    public class UserOut
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public UserProfile? Profile { get; set; }
    }
}
