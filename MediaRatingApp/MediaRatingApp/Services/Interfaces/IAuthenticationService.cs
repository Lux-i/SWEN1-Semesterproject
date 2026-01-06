using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaRatingApp.Models;

namespace MediaRatingApp.Services.Interfaces
{
    public interface IAuthenticationService
    {
        Task<User> RegisterAsync(string username, string password);
        Task<string?> LoginAsync(string username, string password);
        int? ValidateToken(string token);
        bool LogoutUser(int userId);
        Task<bool> UpdateUserPassword(int userId, string password, string newPassword);

        Task<bool> DeleteUserAsync(int userId);
    }
}
