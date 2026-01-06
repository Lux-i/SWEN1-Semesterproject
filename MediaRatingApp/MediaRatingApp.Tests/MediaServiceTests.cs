using MediaRatingApp.Data.Interfaces;
using MediaRatingApp.Models;
using MediaRatingApp.Models.Enums;
using MediaRatingApp.Services.Implementations;
using NSubstitute;
using NUnit.Framework;

namespace MediaRatingApp.Tests.Services
{
    [TestFixture]
    public class MediaServiceTests
    {
        private IMediaRepository _mediaRepo;
        private MediaService _mediaService;

        [SetUp]
        public void Setup()
        {
            _mediaRepo = Substitute.For<IMediaRepository>();
            _mediaService = new MediaService(_mediaRepo);
        }

        [Test]
        public async Task CreateAsync_WithValidMedia_ReturnsNewId()
        {
            // Arrange
            var media = new Media
            {
                Title = "Test Movie",
                MediaType = MediaType.Movie,
                MediaDescription = "A test movie",
            };
            int userId = 1;
            _mediaRepo
                .GetWithFilterAsync(Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(new List<Media>());
            _mediaRepo.CreateAsync(Arg.Any<Media>()).Returns(100);

            // Act
            int result = await _mediaService.CreateAsync(media, userId);

            // Assert
            Assert.That(result, Is.EqualTo(100));
            Assert.That(media.CreatorId, Is.EqualTo(userId));
        }

        [Test]
        public void CreateAsync_WithEmptyTitle_ThrowsArgumentException()
        {
            // Arrange
            var media = new Media { Title = "", MediaType = MediaType.Movie };
            int userId = 1;

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _mediaService.CreateAsync(media, userId)
            );
            Assert.That(ex.Message, Does.Contain("title"));
        }

        [Test]
        public void CreateAsync_WithDuplicateMedia_ThrowsInvalidOperationException()
        {
            // Arrange
            var media = new Media
            {
                Title = "Duplicate Movie",
                MediaType = MediaType.Movie,
                MediaDescription = "Same description",
            };
            var existingMedia = new List<Media> { media };
            _mediaRepo
                .GetWithFilterAsync(Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(existingMedia);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _mediaService.CreateAsync(media, 1)
            );
            Assert.That(ex.Message, Does.Contain("already has an entry"));
        }

        [Test]
        public async Task GetByIdAsync_WithValidId_ReturnsMedia()
        {
            // Arrange
            var expectedMedia = new Media { Id = 1, Title = "Test Movie" };
            _mediaRepo.GetByIdAsync(1).Returns(expectedMedia);

            // Act
            var result = await _mediaService.GetByIdAsync(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(1));
            Assert.That(result.Title, Is.EqualTo("Test Movie"));
        }

        [Test]
        public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            _mediaRepo.GetByIdAsync(999).Returns((Media?)null);

            // Act
            var result = await _mediaService.GetByIdAsync(999);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task UpdateAsync_WithValidOwner_ReturnsTrue()
        {
            // Arrange
            var existingMedia = new Media
            {
                Id = 1,
                Title = "Old Title",
                CreatorId = 1,
                MediaType = MediaType.Movie,
            };
            var updatedMedia = new Media
            {
                Title = "New Title",
                MediaDescription = "New Description",
            };
            _mediaRepo.GetByIdAsync(1).Returns(existingMedia);
            _mediaRepo.UpdateAsync(Arg.Any<Media>()).Returns(true);

            // Act
            var result = await _mediaService.UpdateAsync(1, updatedMedia, 1);

            // Assert
            Assert.That(result, Is.True);
            await _mediaRepo.Received(1).UpdateAsync(Arg.Is<Media>(m => m.Title == "New Title"));
        }

        [Test]
        public void UpdateAsync_WithWrongOwner_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var existingMedia = new Media
            {
                Id = 1,
                Title = "Test",
                CreatorId = 1,
            };
            _mediaRepo.GetByIdAsync(1).Returns(existingMedia);
            var updatedMedia = new Media { Title = "New Title" };

            // Act & Assert
            var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _mediaService.UpdateAsync(1, updatedMedia, 2)
            ); // Different user
            Assert.That(ex.Message, Does.Contain("permission"));
        }

        [Test]
        public async Task UpdateAsync_OnlyUpdatesProvidedFields()
        {
            // Arrange
            var existingMedia = new Media
            {
                Id = 1,
                Title = "Old Title",
                MediaDescription = "Old Description",
                CreatorId = 1,
                ArtworkUrl = "old-url.jpg",
            };
            var updatedMedia = new Media
            {
                Title = "New Title",
                MediaDescription = null,
                ArtworkUrl = null,
            };
            _mediaRepo.GetByIdAsync(1).Returns(existingMedia);
            _mediaRepo.UpdateAsync(Arg.Any<Media>()).Returns(true);

            // Act
            await _mediaService.UpdateAsync(1, updatedMedia, 1);

            // Assert
            await _mediaRepo
                .Received(1)
                .UpdateAsync(
                    Arg.Is<Media>(m =>
                        m.Title == "New Title"
                        && m.MediaDescription == "Old Description"
                        && m.ArtworkUrl == "old-url.jpg"
                    )
                );
        }

        [Test]
        public async Task DeleteAsync_WithValidOwner_ReturnsTrue()
        {
            // Arrange
            var existingMedia = new Media { Id = 1, CreatorId = 1 };
            _mediaRepo.GetByIdAsync(1).Returns(existingMedia);
            _mediaRepo.DeleteAsync(1).Returns(true);

            // Act
            var result = await _mediaService.DeleteAsync(1, 1);

            // Assert
            Assert.That(result, Is.True);
            await _mediaRepo.Received(1).DeleteAsync(1);
        }

        [Test]
        public void DeleteAsync_WithWrongOwner_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var existingMedia = new Media { Id = 1, CreatorId = 1 };
            _mediaRepo.GetByIdAsync(1).Returns(existingMedia);

            // Act & Assert
            Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _mediaService.DeleteAsync(1, 2)
            );
        }

        [Test]
        public async Task IsOwner_WhenUserIsOwner_ReturnsTrue()
        {
            // Arrange
            var media = new Media { Id = 1, CreatorId = 5 };
            _mediaRepo.GetByIdAsync(1).Returns(media);

            // Act
            var result = await _mediaService.IsOwner(1, 5);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task IsOwner_WhenUserIsNotOwner_ReturnsFalse()
        {
            // Arrange
            var media = new Media { Id = 1, CreatorId = 5 };
            _mediaRepo.GetByIdAsync(1).Returns(media);

            // Act
            var result = await _mediaService.IsOwner(1, 10);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task GetAllAsync_ReturnsAllMedia()
        {
            // Arrange
            var mediaList = new List<Media>
            {
                new Media { Id = 1, Title = "Movie 1" },
                new Media { Id = 2, Title = "Movie 2" },
                new Media { Id = 3, Title = "Game 1" },
            };
            _mediaRepo.GetAllAsync().Returns(mediaList);

            // Act
            var result = await _mediaService.GetAllAsync();

            // Assert
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result[0].Title, Is.EqualTo("Movie 1"));
        }

        [Test]
        public async Task CreateAsync_SetsCreatorIdCorrectly()
        {
            // Arrange
            var media = new Media { Title = "Test", MediaType = MediaType.Book };
            _mediaRepo
                .GetWithFilterAsync(Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(new List<Media>());
            _mediaRepo.CreateAsync(Arg.Any<Media>()).Returns(50);

            // Act
            await _mediaService.CreateAsync(media, 42);

            // Assert
            await _mediaRepo.Received(1).CreateAsync(Arg.Is<Media>(m => m.CreatorId == 42));
        }

        [Test]
        public async Task UpdateAsync_PreservesEmptyStringTitle()
        {
            // Arrange
            var existingMedia = new Media
            {
                Id = 1,
                Title = "Original Title",
                CreatorId = 1,
            };
            var updatedMedia = new Media { Title = "" };
            _mediaRepo.GetByIdAsync(1).Returns(existingMedia);
            _mediaRepo.UpdateAsync(Arg.Any<Media>()).Returns(true);

            // Act
            await _mediaService.UpdateAsync(1, updatedMedia, 1);

            // Assert
            await _mediaRepo
                .Received(1)
                .UpdateAsync(Arg.Is<Media>(m => m.Title == "Original Title"));
        }
    }
}
