using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaRatingApp.Models;

namespace MediaRatingApp.Services.Interfaces
{
    interface IUserService
    {
        Task<List<UserProfileSlim>> GetAllUserProfilesAsync();
        Task<UserOut> GetUserByIdAsync(int userId);
        Task<UserOut> GetUserByUsernameAsync(string username);
        Task<UserProfile> GetUserProfileByIdAsync(int userId);
        Task<bool> UpdateUserProfileAsync(int userId, UserProfile profile);

        /// <summary>
        /// Returns the user along with their profile information by propagating User.Profile.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<UserOut> GetUserFullByIdAsync(int userId);
        Task<UserOut> GetUserFullByUsernameAsync(string username);
    }
}
