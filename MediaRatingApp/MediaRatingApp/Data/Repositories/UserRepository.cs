using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaRatingApp.Data.Interfaces;
using MediaRatingApp.Models;
using Npgsql;

namespace MediaRatingApp.Data.Repositories
{
    class UserRepository : IUserRepository
    {
        private readonly NpgsqlDataSource _dataSource;

        public UserRepository(NpgsqlDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        private async Task<NpgsqlConnection> OpenConnectionAsync() =>
            await _dataSource.OpenConnectionAsync();

        public async Task<List<User>> GetAllAsync()
        {
            const string sql = """
                SELECT id, username, pw_hash, created_at
                FROM users;
                """;
            var users = new List<User>();
            await using var conn = await OpenConnectionAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                users.Add(
                    new User
                    {
                        Id = reader.GetInt32(0),
                        Username = reader.GetString(1),
                        PwHash = reader.GetString(2),
                        CreatedAt = reader.GetDateTime(3),
                    }
                );
            }
            return users;
        }

        public async Task<List<UserProfileSlim>> GetAllProfilesSlimAsync()
        {
            // Get all user profiles (slim data) (exclude DELETED user with user_id = 0)
            const string sql = """
                SELECT user_id, display_name, avatar_url
                FROM user_profiles
                WHERE user_id != 0;
                """;
            var profiles = new List<UserProfileSlim>();
            await using var conn = await OpenConnectionAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                profiles.Add(
                    new UserProfileSlim
                    {
                        UserId = reader.GetInt32(0),
                        DisplayName = reader.IsDBNull(1) ? null : reader.GetString(1),
                        AvatarUrl = reader.IsDBNull(2) ? null : reader.GetString(2),
                    }
                );
            }
            return profiles;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            const string sql = """
                SELECT id, username, pw_hash, created_at
                FROM users
                WHERE id = @id;
                """;

            await using var conn = await OpenConnectionAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", id);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            return new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                PwHash = reader.GetString(2),
                CreatedAt = reader.GetDateTime(3),
            };
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            const string sql = """
                SELECT id, username, pw_hash, created_at
                FROM users
                WHERE username = @username;
                """;

            await using var conn = await OpenConnectionAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("username", username);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            return new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                PwHash = reader.GetString(2),
                CreatedAt = reader.GetDateTime(3),
            };
        }

        public async Task<int> CreateAsync(User user)
        {
            const string sql = """
                INSERT INTO users (username, pw_hash)
                VALUES (@username, @pw_hash)
                RETURNING id;
                """;

            await using var conn = await OpenConnectionAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("username", user.Username);
            cmd.Parameters.AddWithValue("pw_hash", user.PwHash);

            return (int)(await cmd.ExecuteScalarAsync())!;
        }

        public async Task<bool> UpdateAsync(User user)
        {
            const string sql = """
                UPDATE users
                SET username = @username,
                    pw_hash = @pw_hash
                WHERE id = @id;
                """;

            await using var conn = await OpenConnectionAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", user.Id);
            cmd.Parameters.AddWithValue("username", user.Username);
            cmd.Parameters.AddWithValue("pw_hash", user.PwHash);

            return await cmd.ExecuteNonQueryAsync() == 1;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = """
                DELETE FROM users
                WHERE id = @id;
                """;

            await using var conn = await OpenConnectionAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", id);

            return await cmd.ExecuteNonQueryAsync() == 1;
        }

        public async Task<bool> ExistsAsync(string username)
        {
            const string sql = """
                SELECT 1
                FROM users
                WHERE username = @username;
                """;

            await using var conn = await OpenConnectionAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("username", username);

            return await cmd.ExecuteScalarAsync() != null;
        }

        public async Task<int> CreateProfileAsync(UserProfile profile)
        {
            const string sql = """
                INSERT INTO user_profiles
                    (user_id, display_name, bio, avatar_url)
                VALUES
                    (@user_id, @display_name, @bio, @avatar_url)
                RETURNING user_id;
                """;
            await using var conn = await OpenConnectionAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("user_id", profile.UserId);
            cmd.Parameters.AddWithValue(
                "display_name",
                (object?)profile.DisplayName ?? DBNull.Value
            );
            cmd.Parameters.AddWithValue("bio", (object?)profile.Bio ?? DBNull.Value);
            cmd.Parameters.AddWithValue("avatar_url", (object?)profile.AvatarUrl ?? DBNull.Value);
            return (int)(await cmd.ExecuteScalarAsync())!;
        }

        public async Task<UserProfile?> GetProfileAsync(int userId)
        {
            const string sql = """
                SELECT user_id, display_name, bio, avatar_url, total_ratings
                FROM user_profiles
                WHERE user_id = @user_id;
                """;

            await using var conn = await OpenConnectionAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("user_id", userId);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            return new UserProfile
            {
                UserId = reader.GetInt32(0),
                DisplayName = reader.IsDBNull(1) ? null : reader.GetString(1),
                Bio = reader.IsDBNull(2) ? null : reader.GetString(2),
                AvatarUrl = reader.IsDBNull(3) ? null : reader.GetString(3),
                TotalRatings = reader.GetInt32(4),
            };
        }

        public async Task<bool> UpdateProfileAsync(UserProfile profile)
        {
            const string sql = """
                UPDATE user_profiles
                SET display_name = @display_name,
                    bio = @bio,
                    avatar_url = @avatar_url
                WHERE user_id = @user_id;
                """;

            await using var conn = await OpenConnectionAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("user_id", profile.UserId);
            cmd.Parameters.AddWithValue(
                "display_name",
                (object?)profile.DisplayName ?? DBNull.Value
            );
            cmd.Parameters.AddWithValue("bio", (object?)profile.Bio ?? DBNull.Value);
            cmd.Parameters.AddWithValue("avatar_url", (object?)profile.AvatarUrl ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
            return true;
        }
    }

    public class FakeUserRepository : IUserRepository
    {
        private static ConcurrentDictionary<int, User> _users = new();
        private static int _nextId = 1;

        private static ConcurrentDictionary<int, UserProfile> _profiles = new();

        public async Task<List<User>> GetAllAsync()
        {
            return _users.Values.ToList();
        }

        public async Task<List<UserProfileSlim>> GetAllProfilesSlimAsync()
        {
            return _profiles
                .Values.Select(p => new UserProfileSlim
                {
                    UserId = p.UserId,
                    DisplayName = p.DisplayName,
                    AvatarUrl = p.AvatarUrl,
                })
                .ToList();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            _users.TryGetValue(id, out var user);
            return user;
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            var user = _users.Values.FirstOrDefault(u => u.Username == username);
            return user;
        }

        public async Task<int> CreateAsync(User user)
        {
            int id = Interlocked.Increment(ref _nextId);
            var newUser = new User
            {
                Id = id,
                Username = user.Username,
                PwHash = user.PwHash,
                CreatedAt = DateTime.Now,
            };
            return id;
        }

        public async Task<bool> UpdateAsync(User user)
        {
            if (_users.ContainsKey(user.Id))
            {
                _users[user.Id] = user;
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return _users.TryRemove(id, out _);
        }

        public async Task<bool> ExistsAsync(string username)
        {
            bool exists = _users.Values.Any(u => u.Username == username);
            return exists;
        }

        public async Task<int> CreateProfileAsync(UserProfile profile)
        {
            _profiles[profile.UserId] = profile;
            return profile.UserId;
        }

        public async Task<UserProfile?> GetProfileAsync(int userId)
        {
            _profiles.TryGetValue(userId, out var profile);
            return profile;
        }

        public async Task<bool> UpdateProfileAsync(UserProfile profile)
        {
            _profiles[profile.UserId] = profile;
            return true;
        }
    }
}
