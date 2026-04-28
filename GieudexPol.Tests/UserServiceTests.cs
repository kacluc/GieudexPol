using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using Moq;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Test class for the <see cref="UserService"/> service, using mocked repository interactions.
/// </summary>
public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly UserService _userService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserServiceTests"/> class.
    /// Sets up mock dependencies for testing purposes.
    /// </summary>
    public UserServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _userService = new UserService(_mockUserRepository.Object);
    }

    /// <summary>
    /// Tests that GetByIdAsync returns the expected user when a valid ID is provided.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_ReturnsUserWhenExists()
    {
        // Arrange
        var expectedUser = new User { Id = 1, Username = "testuser" };
        _mockUserRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _userService.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("testuser", result.Username);
        _mockUserRepository.Verify(r => r.GetByIdAsync(1), Times.Once());
    }

    /// <summary>
    /// Tests that GetByIdAsync returns null when the user ID does not exist in the repository.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_ReturnsNullWhenNotFound()
    {
        // Arrange
        _mockUserRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((User)null);

        // Act
        var result = await _userService.GetByIdAsync(99);

        // Assert
        Assert.Null(result);
        _mockUserRepository.Verify(r => r.GetByIdAsync(99), Times.Once());
    }

    /// <summary>
    /// Tests that GetAllAsync returns a collection containing all existing users.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_ReturnsAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = 1, Username = "userA" },
            new User { Id = 2, Username = "userB" }
        };
        _mockUserRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(users);

        // Act
        var result = await _userService.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _mockUserRepository.Verify(r => r.GetAllAsync(), Times.Once());
    }

    /// <summary>
    /// Tests that calling AddAsync correctly calls the repository's AddAsync method.
    /// </summary>
    [Fact]
    public async Task AddAsync_CallsRepositoryAdd()
    {
        // Arrange
        var newUser = new User { Username = "newuser" };

        // Act
        await _userService.AddAsync(newUser);

        // Assert
        _mockUserRepository.Verify(r => r.AddAsync(newUser), Times.Once());
    }

    /// <summary>
    /// Tests that calling UpdateAsync correctly calls the repository's UpdateAsync method.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_CallsRepositoryUpdate()
    {
        // Arrange
        var updatedUser = new User { Id = 1, Username = "updateduser" };

        // Act
        await _userService.UpdateAsync(updatedUser);

        // Assert
        _mockUserRepository.Verify(r => r.UpdateAsync(updatedUser), Times.Once());
    }

    /// <summary>
    /// Tests that calling DeleteAsync correctly calls the repository's DeleteAsync method.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_CallsRepositoryDelete()
    {
        // Arrange
        var userToDelete = new User { Id = 1 };

        // Act
        await _userService.DeleteAsync(userToDelete);

        // Assert
        _mockUserRepository.Verify(r => r.DeleteAsync(userToDelete), Times.Once());
    }

    /// <summary>
    /// Tests that GetByUsernameAsync returns the expected user when a valid username is provided.
    /// </summary>
    [Fact]
    public async Task GetByUsernameAsync_ReturnsUserWhenExists()
    {
        // Arrange
        var expectedUser = new User { Id = 2, Username = "testbyusername" };
        _mockUserRepository.Setup(r => r.GetByUsernameAsync("testbyusername"))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _userService.GetByUsernameAsync("testbyusername");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("testbyusername", result.Username);
        _mockUserRepository.Verify(r => r.GetByUsernameAsync("testbyusername"), Times.Once());
    }

    /// <summary>
    /// Tests that GetByUsernameAsync returns null when the username does not exist in the repository.
    /// </summary>
    [Fact]
    public async Task GetByUsernameAsync_ReturnsNullWhenNotFound()
    {
        // Arrange
        _mockUserRepository.Setup(r => r.GetByUsernameAsync("nonexistentuser"))
            .ReturnsAsync((User)null);

        // Act
        var result = await _userService.GetByUsernameAsync("nonexistentuser");

        // Assert
        Assert.Null(result);
        _mockUserRepository.Verify(r => r.GetByUsernameAsync("nonexistentuser"), Times.Once());
    }
}

