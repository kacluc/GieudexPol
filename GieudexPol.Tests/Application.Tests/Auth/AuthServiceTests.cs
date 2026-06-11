using GieudexPol.Application.Auth.Commands;
using GieudexPol.Application.Auth.DTOs;
using GieudexPol.Application.Auth.Services;
using GieudexPol.Domain.Auth;
using GieudexPol.Infrastructure.Auth;
using Microsoft.AspNetCore.Identity;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GieudexPol.Tests.Application.Tests.Auth
{
    // 1. Klasy pomocnicze (Wrappery), które pozwalają testom "gadać" z nowym serwisem 
    // przy użyciu starych mocków Twojego znajomego, bez dotykania kodu produkcyjnego.
    public class FakeIdentityService : IIdentityService
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManager;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManager;

        public FakeIdentityService(Mock<UserManager<ApplicationUser>> userManager, Mock<SignInManager<ApplicationUser>> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<bool> CheckPasswordAsync(string email, string password)
        {
            var appUser = await _userManager.Object.FindByEmailAsync(email);
            if (appUser == null) return false;
            var result = await _signInManager.Object.CheckPasswordSignInAsync(appUser, password, false);
            return result.Succeeded;
        }
    }

    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IJwtService> _mockJwtService;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockJwtService = new Mock<IJwtService>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);
            _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
                _mockUserManager.Object, Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>(), Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(), null!, null!, null!, null!);

            // 2. Mapujemy stare mocki znajomego na interfejsy nowego serwisu za pomocą naszych wrapperów
            var adapterIdentity = new FakeIdentityService(_mockUserManager, _mockSignInManager);

            _authService = new AuthService(
                _mockUserRepository.Object,
                _mockJwtService.Object,
                adapterIdentity);
        }

        [Fact]
        public async Task RegisterUserCommand_ShouldRegisterNewUserAndReturnAuthResponse()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                DisplayName = "Test User",
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };
            var command = new RegisterUserCommand { RegisterRequest = registerRequest };

            _mockUserRepository.Setup(r => r.GetByEmailAsync(registerRequest.Email))
                               .ReturnsAsync((User?)null);
            _mockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>()))
                               .Callback<User>(user => user.AssignApplicationUserId(7))
                               .Returns(Task.CompletedTask);
            _mockJwtService.Setup(j => j.GenerateToken(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    "User"))
                           .Returns("dummy_jwt_token");

            // Act
            var response = await _authService.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
            Assert.Equal("dummy_jwt_token", response.Token);
            Assert.Equal(registerRequest.Email, response.Email);
            Assert.Equal(7, response.UserId);
            Assert.Equal("User", response.Role);
            _mockUserRepository.Verify(r => r.AddAsync(
                It.Is<User>(user => user.DisplayName == registerRequest.DisplayName)), Times.Once);
            _mockUserRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task RegisterUserCommand_ShouldThrowUserAlreadyExistsException_WhenUserExists()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                DisplayName = "Test User",
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };
            var command = new RegisterUserCommand { RegisterRequest = registerRequest };
            var existingUser = new User(Guid.NewGuid(), registerRequest.Email, "hashedpassword");

            _mockUserRepository.Setup(r => r.GetByEmailAsync(registerRequest.Email))
                               .ReturnsAsync(existingUser);

            // Act & Assert
            await Assert.ThrowsAsync<UserAlreadyExistsException>(
                () => _authService.Handle(command, CancellationToken.None));
            _mockUserRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task LoginUserCommand_ShouldReturnAuthResponse_ForValidCredentials()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "test@example.com",
                Password = "Password123!"
            };
            var command = new LoginUserCommand { LoginRequest = loginRequest };

            var applicationUser = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = loginRequest.Email, UserName = loginRequest.Email };
            _mockUserManager.Setup(u => u.FindByEmailAsync(loginRequest.Email))
                            .ReturnsAsync(applicationUser);
            _mockSignInManager.Setup(s => s.CheckPasswordSignInAsync(applicationUser, loginRequest.Password, false))
                              .ReturnsAsync(SignInResult.Success);
            _mockUserRepository.Setup(r => r.GetByEmailAsync(loginRequest.Email))
                               .ReturnsAsync(new User(Guid.Parse(applicationUser.Id), applicationUser.Email, "hashedPassword", 7));
            _mockJwtService.Setup(j => j.GenerateToken(
                    applicationUser.Id,
                    loginRequest.Email,
                    "User"))
                           .Returns("dummy_jwt_token");

            // Act
            var response = await _authService.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
            Assert.Equal("dummy_jwt_token", response.Token);
            Assert.Equal(loginRequest.Email, response.Email);
            Assert.Equal(7, response.UserId);
            Assert.Equal("User", response.Role);
        }

        [Fact]
        public async Task LoginUserCommand_ShouldThrowInvalidCredentialsException_WhenUserNotFound()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "test@example.com",
                Password = "Password123!"
            };
            var command = new LoginUserCommand { LoginRequest = loginRequest };

            _mockUserManager.Setup(u => u.FindByEmailAsync(loginRequest.Email))
                            .ReturnsAsync((ApplicationUser?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidCredentialsException>(
                () => _authService.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task LoginUserCommand_ShouldThrowInvalidCredentialsException_ForInvalidPassword()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "test@example.com",
                Password = "WrongPassword"
            };
            var command = new LoginUserCommand { LoginRequest = loginRequest };

            var applicationUser = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = loginRequest.Email, UserName = loginRequest.Email };
            _mockUserManager.Setup(u => u.FindByEmailAsync(loginRequest.Email))
                            .ReturnsAsync(applicationUser);
            _mockSignInManager.Setup(s => s.CheckPasswordSignInAsync(applicationUser, loginRequest.Password, false))
                              .ReturnsAsync(SignInResult.Failed);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidCredentialsException>(
                () => _authService.Handle(command, CancellationToken.None));
        }
    }
}
