using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaRatingApp.Models;

namespace MediaRatingApp.Services.Interfaces
{
    interface IAuthenticationService
    {
        Task<User> RegisterAsync(string username, string password, string email);
        Task<string?> LoginAsync(string username, string password);
    }
}
