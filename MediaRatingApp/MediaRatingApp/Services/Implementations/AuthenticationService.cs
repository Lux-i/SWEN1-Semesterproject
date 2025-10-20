using MediaRatingApp.Data.Interfaces;
using MediaRatingApp.Services.Interfaces;
using MediaRatingApp.Models;
using System.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace MediaRatingApp.Services.Implementations
{
    public class AuthenticationService : IAuthenticationService
    {
        private const int SaltSize = 16; // 16 Byte => 128 bit
        private const int HashSize = 32; // 32 Byte => 256 bit
        private const int Iterations = 100000; // Number of PBKDF2 iterations

        private readonly IUserRepository _userRepository;
        private static ConcurrentDictionary<string, int> _tokens = new();

        public AuthenticationService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> RegisterAsync(string username, string password, string email)
        {
            #region Input Validation
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be empty", nameof(username));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty", nameof(password));

            if (password.Length < 8)
                throw new ArgumentException("Password must be at least 8 characters", nameof(password));
            #endregion

            if (await _userRepository.ExistsAsync(username))
            {
                throw new InvalidOperationException("Username is taken!");
            }

            string passwordHash = HashPassword(password);

            User newUser = new User(username, passwordHash, email);

            int userId = await _userRepository.CreateAsync(newUser);

            User? createdUser = await _userRepository.GetByIdAsync(userId);

            if (createdUser != null)
            {
                return createdUser;
            }
            else
            {
                throw new Exception("User registration failed.");
            }
        }

        public async Task<string?> LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            User? user = await _userRepository.GetByUsernameAsync(username);

            if (user == null || !VerifyPassword(password, user.PasswordHash))
            {
                return null;
            }

            string token = Guid.NewGuid().ToString();
            _tokens[token] = user._Id;
            return token;
        }

        public int? ValidateToken(string token)
        {
            return _tokens.TryGetValue(token, out int userId) ? userId : null;
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
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
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
    }
}
