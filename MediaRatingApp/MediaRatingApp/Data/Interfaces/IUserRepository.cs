using MediaRatingApp.Models;

namespace MediaRatingApp.Data.Interfaces
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllAsync();

        Task<List<UserProfileSlim>> GetAllProfilesSlimAsync();

        Task<User?> GetByIdAsync(int id);

        Task<User?> GetByUsernameAsync(string username);

        Task<int> CreateAsync(User user);

        Task<bool> UpdateAsync(User user);

        Task<bool> DeleteAsync(int id);

        Task<bool> ExistsAsync(string username);

        Task<int> CreateProfileAsync(UserProfile profile);

        Task<UserProfile?> GetProfileAsync(int userId);

        Task<bool> UpdateProfileAsync(UserProfile profile);
    }
}
