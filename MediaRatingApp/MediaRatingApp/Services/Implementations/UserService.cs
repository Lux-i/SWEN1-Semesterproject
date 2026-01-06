using MediaRatingApp.Data.Interfaces;
using MediaRatingApp.Models;
using MediaRatingApp.Models.Mappers;
using MediaRatingApp.Services.Interfaces;

namespace MediaRatingApp.Services.Implementations
{
    class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;

        public UserService(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<List<UserProfileSlim>> GetAllUserProfilesAsync()
        {
            List<UserProfileSlim> userProfiles = await _userRepo.GetAllProfilesSlimAsync();
            return userProfiles;
        }

        public async Task<UserOut> GetUserByIdAsync(int userId)
        {
            User? user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            return UserModelMapper.ToUserOut(user);
        }

        public async Task<UserOut> GetUserByUsernameAsync(string username)
        {
            User? user = await _userRepo.GetByUsernameAsync(username);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            return UserModelMapper.ToUserOut(user);
        }

        public async Task<UserProfile> GetUserProfileByIdAsync(int userId)
        {
            UserProfile? profile = await _userRepo.GetProfileAsync(userId);
            if (profile == null)
            {
                throw new Exception("User profile not found");
            }

            return profile;
        }

        public async Task<bool> UpdateUserProfileAsync(int userId, UserProfile profile)
        {
            UserProfile existingProfile = await GetUserProfileByIdAsync(userId);

            // Validate DisplayName not being an empty string
            if (String.IsNullOrEmpty(profile.DisplayName))
            {
                throw new ArgumentException("DisplayName cannot be an empty string");
            }

            // Set profile's UserId to given userId and retain existing values for null fields
            UserProfile newProfile = new UserProfile
            {
                UserId = userId,
                DisplayName = profile.DisplayName ?? existingProfile.DisplayName,
                Bio = profile.Bio ?? existingProfile.Bio,
                AvatarUrl = profile.AvatarUrl ?? existingProfile.AvatarUrl,
            };

            return await _userRepo.UpdateProfileAsync(newProfile);
        }

        public async Task<UserOut> GetUserFullByIdAsync(int userId)
        {
            UserOut user = await GetUserByIdAsync(userId);
            UserProfile profile = await GetUserProfileByIdAsync(userId);

            // Set reference links
            user.Profile = profile;
            profile.User = UserModelMapper.ToUser(user);

            return user;
        }

        public async Task<UserOut> GetUserFullByUsernameAsync(string username)
        {
            UserOut user = await GetUserByUsernameAsync(username);
            UserProfile profile = await GetUserProfileByIdAsync(user.Id);

            // Set reference links
            user.Profile = profile;
            profile.User = UserModelMapper.ToUser(user);

            return user;
        }
    }
}
