using MediaRatingApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatingApp.Services.Interfaces
{
    interface IUserService
    {
        Task<User> CreateUserAsync(string username, string password, string email);
        Task<User> GetUserByIdAsync(int userId);
        Task<User> GetUserByUsernameAsync(string username);
        Task UpdateUser(int userId, string? username, string password, string email);
        Task DeleteUserAsync(int userId);
    }
}
