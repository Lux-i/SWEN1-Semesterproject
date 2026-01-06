using MediaRatingApp.Data.Interfaces;
using MediaRatingApp.Models;
using MediaRatingApp.Services.Implementations;
using NSubstitute;

namespace MediaRatingApp.Tests.Services
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private IUserRepository _userRepo;
        private AuthenticationService _authService;

        [SetUp]
        public void Setup()
        {
            _userRepo = Substitute.For<IUserRepository>();
            _authService = new AuthenticationService(_userRepo);
        }

        [Test]
        public void RegisterAsync_WithEmptyUsername_ThrowsArgumentException()
        {
            // Arrange
            string username = "";
            string password = "ValidPass123!";

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _authService.RegisterAsync(username, password)
            );
            Assert.That(ex.ParamName, Is.EqualTo("username"));
        }

        [Test]
        public void RegisterAsync_WithEmptyPassword_ThrowsArgumentException()
        {
            // Arrange
            string username = "testuser";
            string password = "";

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _authService.RegisterAsync(username, password)
            );
            Assert.That(ex.ParamName, Is.EqualTo("password"));
        }

        [Test]
        public void RegisterAsync_WithWeakPassword_ThrowsArgumentException()
        {
            // Arrange
            string username = "testuser";
            string password = "weakpassword";

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () =>
                await _authService.RegisterAsync(username, password)
            );
        }

        [Test]
        public async Task LoginAsync_WithInvalidUsername_ReturnsNull()
        {
            // Arrange
            _userRepo.GetByUsernameAsync(Arg.Any<string>()).Returns((User?)null);

            // Act
            var token = await _authService.LoginAsync("nonexistent", "password");

            // Assert
            Assert.That(token, Is.Null);
        }

        [Test]
        public async Task LoginAsync_WithInvalidPassword_ReturnsNull()
        {
            // Arrange
            string username = "testuser";
            string correctPassword = "ValidPass123!";

            // Register user with correct password
            _userRepo.CreateAsync(Arg.Any<User>()).Returns(1);
            _userRepo
                .GetByIdAsync(1)
                .Returns(
                    new User
                    {
                        Id = 1,
                        Username = username,
                        PwHash = "hash",
                    }
                );
            _userRepo.CreateProfileAsync(Arg.Any<UserProfile>()).Returns(1);
            var user = await _authService.RegisterAsync(username, correctPassword);

            _userRepo.GetByUsernameAsync(username).Returns(user);

            // Act - try login with wrong password
            var token = await _authService.LoginAsync(username, "InvalidPass123!_becauseitswrong");

            // Assert
            Assert.That(token, Is.Null);
        }

        [Test]
        public void ValidateToken_WithInvalidToken_ReturnsNull()
        {
            // Arrange
            string fakeToken = "abcdefg";

            // Act
            var userId = _authService.ValidateToken(fakeToken);

            // Assert
            Assert.That(userId, Is.Null);
        }

        [Test]
        public async Task UpdateUserPassword_WithWeakNewPassword_ThrowsArgumentException()
        {
            // Arrange
            string username = "testuser";
            string oldPassword = "ValidPass123!";

            _userRepo.CreateAsync(Arg.Any<User>()).Returns(1);
            _userRepo
                .GetByIdAsync(1)
                .Returns(
                    new User
                    {
                        Id = 1,
                        Username = username,
                        PwHash = "hash",
                    }
                );
            _userRepo.CreateProfileAsync(Arg.Any<UserProfile>()).Returns(1);
            var user = await _authService.RegisterAsync(username, oldPassword);

            _userRepo.GetByIdAsync(1).Returns(user);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () =>
                await _authService.UpdateUserPassword(1, oldPassword, "weakpassword")
            );
        }

        [Test]
        public async Task DeleteUserAsync_WithValidUserId_ReturnsTrue()
        {
            // Arrange
            _userRepo.DeleteAsync(1).Returns(true);

            // Act
            var result = await _authService.DeleteUserAsync(1);

            // Assert
            Assert.That(result, Is.True);
            await _userRepo.Received(1).DeleteAsync(1);
        }
    }
}
