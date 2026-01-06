using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using MediaRatingApp.Data.Interfaces;
using MediaRatingApp.Models;
using MediaRatingApp.Services.Interfaces;
using Npgsql;

namespace MediaRatingApp.Services.Implementations
{
    public class AuthenticationService : IAuthenticationService
    {
        private const int SaltSize = 16; // 16 Byte => 128 bit
        private const int HashSize = 32; // 32 Byte => 256 bit
        private const int Iterations = 100000; // Number of PBKDF2 iterations

        // (?=.*<toEnsure>) -> Positive lookahead to ensure at least one occurrence of <toEnsure>
        // Special character set: @ $ ! % * ? & _
        private static readonly Regex PasswordRegex = new(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&_]).{8,}$",
            RegexOptions.Compiled
        );

        private readonly IUserRepository _userRepository;
        private static ConcurrentDictionary<string, int> _tokens = new();

        public AuthenticationService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> RegisterAsync(string username, string password)
        {
            #region Input Validation
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be empty", nameof(username));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty", nameof(password));

            ValidatePasswordStrength(password);
            #endregion

            string passwordHash = HashPassword(password);

            User newUser = new User { Username = username, PwHash = passwordHash };

            int userId;

            try
            {
                userId = await _userRepository.CreateAsync(newUser);
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                throw new InvalidOperationException("Username is taken!", ex);
            }

            User? createdUser = await _userRepository.GetByIdAsync(userId);

            if (createdUser != null)
            {
                // After creating the user entry, create default profile entry
                createdUser.Profile = new UserProfile
                {
                    UserId = createdUser.Id,
                    DisplayName = username,
                    Bio = string.Empty,
                    AvatarUrl = string.Empty,
                };

                await _userRepository.CreateProfileAsync(createdUser.Profile);

                return createdUser;
            }
            else
            {
                throw new Exception("User registration / confirmation failed.");
            }
        }

        public async Task<string?> LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            User? user = await _userRepository.GetByUsernameAsync(username);

            if (user == null || !VerifyPassword(password, user.PwHash))
            {
                return null;
            }

            // Remove existing token if any
            var existingToken = GetTokenFromUserId(user.Id);
            if (existingToken != null)
            {
                _tokens.TryRemove(existingToken, out _);
            }

            // Generate new token
            string token = Guid.NewGuid().ToString();
            _tokens[token] = user.Id;

            return token;
        }

        public int? ValidateToken(string token)
        {
            return _tokens.TryGetValue(token, out int userId) ? userId : null;
        }

        public bool LogoutUser(int userId)
        {
            var token = GetTokenFromUserId(userId);
            if (token != null)
            {
                return _tokens.TryRemove(token, out _);
            }
            return false;
        }

        public async Task<bool> UpdateUserPassword(int userId, string password, string newPassword)
        {
            ValidatePasswordStrength(newPassword);

            User? user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found!");
            }
            if (!VerifyPassword(password, user.PwHash))
            {
                throw new UnauthorizedAccessException("Wrong Credentials!");
            }

            string newHashedPassword = HashPassword(newPassword);
            user.PwHash = newHashedPassword;
            bool updated = await _userRepository.UpdateAsync(user);
            if (updated)
            {
                // Logout user after password change
                LogoutUser(userId);
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            bool deleted = await _userRepository.DeleteAsync(userId);
            if (deleted)
            {
                // Logout user after deletion
                LogoutUser(userId);
                return true;
            }
            else
            {
                throw new Exception("User deletion failed.");
            }
        }

        private string HashPassword(string password)
        {
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            byte[] hash = HashPasswordWithSalt(password, salt, Iterations);

            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        private byte[] HashPasswordWithSalt(string password, byte[] salt, int iterations)
        {
            using (
                var pbkdf2 = new Rfc2898DeriveBytes(
                    password,
                    salt,
                    iterations,
                    HashAlgorithmName.SHA256
                )
            )
            {
                return pbkdf2.GetBytes(HashSize);
            }
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            try
            {
                string[] parts = storedHash.Split('.');

                if (parts.Length != 3)
                {
                    return false;
                }

                int iterations = int.Parse(parts[0]);
                byte[] salt = Convert.FromBase64String(parts[1]);
                byte[] hash = Convert.FromBase64String(parts[2]);

                byte[] hashToTest = HashPasswordWithSalt(password, salt, iterations);

                return CryptographicOperations.FixedTimeEquals(hash, hashToTest);
            }
            catch
            {
                return false;
            }
        }

        private string? GetTokenFromUserId(int userId)
        {
            var token = _tokens.FirstOrDefault(kvp => kvp.Value == userId).Key;
            return token;
        }

        /// <summary>
        /// Validates that the specified password meets minimum strength requirements.
        /// </summary>
        /// <param name="password">The password string to validate. Must contain at least eight characters, including at least one uppercase
        /// letter, one lowercase letter, one digit, and one special character.</param>
        /// <exception cref="ArgumentException">Thrown if the password does not meet the requirements.</exception>
        private void ValidatePasswordStrength(string password)
        {
            if (!PasswordRegex.IsMatch(password))
                throw new ArgumentException(
                    "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one digit, and one special character.",
                    nameof(password)
                );
        }
    }
}
