using Xunit;
using Moq;
using FluentAssertions;
using GieudexPol.Application.Interfaces;
using GieudexPol.Application.Services;
using GieudexPol.Domain.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GieudexPol.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _userService = new UserService(_mockUserRepository.Object);
        }

        [Fact]
        public async Task AddAsync_ShouldCallRepositoryAddAsync()
        {
            // Arrange
            var user = new User { Username = "testuser", PasswordHash = "hashedpassword" };
            _mockUserRepository.Setup(repo => repo.AddAsync(user)).Returns(Task.CompletedTask);

            // Act
            await _userService.AddAsync(user);

            // Assert
            _mockUserRepository.Verify(repo => repo.AddAsync(user), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var userId = 1;
            var expectedUser = new User { Id = userId, Username = "testuser", PasswordHash = "hashedpassword" };
            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(expectedUser);

            // Act
            var result = await _userService.GetByIdAsync(userId);

            // Assert
            result.Should().BeEquivalentTo(expectedUser);
            _mockUserRepository.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = 1;
            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync((User?)null);

            // Act
            var result = await _userService.GetByIdAsync(userId);

            // Assert
            result.Should().BeNull();
            _mockUserRepository.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetByUsernameAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var username = "testuser";
            var expectedUser = new User { Id = 1, Username = username, PasswordHash = "hashedpassword" };
            _mockUserRepository.Setup(repo => repo.GetByUsernameAsync(username)).ReturnsAsync(expectedUser);

            // Act
            var result = await _userService.GetByUsernameAsync(username);

            // Assert
            result.Should().BeEquivalentTo(expectedUser);
            _mockUserRepository.Verify(repo => repo.GetByUsernameAsync(username), Times.Once);
        }

        [Fact]
        public async Task GetByUsernameAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var username = "nonexistent";
            _mockUserRepository.Setup(repo => repo.GetByUsernameAsync(username)).ReturnsAsync((User?)null);

            // Act
            var result = await _userService.GetByUsernameAsync(username);

            // Assert
            result.Should().BeNull();
            _mockUserRepository.Verify(repo => repo.GetByUsernameAsync(username), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldCallRepositoryUpdateAsync()
        {
            // Arrange
            var user = new User { Id = 1, Username = "updateduser", PasswordHash = "newhashedpassword" };
            _mockUserRepository.Setup(repo => repo.UpdateAsync(user)).Returns(Task.CompletedTask);

            // Act
            await _userService.UpdateAsync(user);

            // Assert
            _mockUserRepository.Verify(repo => repo.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallRepositoryDeleteAsync()
        {
            // Arrange
            var user = new User { Id = 1, Username = "testuser", PasswordHash = "hashedpassword" };
            _mockUserRepository.Setup(repo => repo.DeleteAsync(user)).Returns(Task.CompletedTask);

            // Act
            await _userService.DeleteAsync(user);

            // Assert
            _mockUserRepository.Verify(repo => repo.DeleteAsync(user), Times.Once);
        }
    }
}
