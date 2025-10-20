using MediaRatingApp.Data.Interfaces;
using MediaRatingApp.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatingApp.Data.Repositories
{
    class UserRepository
    {
    }

    public class FakeUserRepository : IUserRepository
    {
        private static ConcurrentDictionary<int, User> _users = new();
        private static int _nextId = 1;

        public Task<User?> GetByIdAsync(int id)
        {
            _users.TryGetValue(id, out var user);
            return Task.FromResult(user);
        }

        public Task<User?> GetByUsernameAsync(string username)
        {
            var user = _users.Values.FirstOrDefault(u => u.Username == username);
            return Task.FromResult(user);
        }

        public Task<int> CreateAsync(User user)
        {
            int id = Interlocked.Increment(ref _nextId);
            var newUser = new User(user.Username, user.PasswordHash, user.Email);
            newUser._Id = id;
            _users[id] = newUser;
            return Task.FromResult(id);
        }

        public Task<bool> UpdateAsync(User user)
        {
            if (_users.ContainsKey(user._Id))
            {
                _users[user._Id] = user;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<bool> DeleteAsync(int id)
        {
            return Task.FromResult(_users.TryRemove(id, out _));
        }

        public Task<bool> ExistsAsync(string username)
        {
            bool exists = _users.Values.Any(u => u.Username == username);
            return Task.FromResult(exists);
        }
    }
}
