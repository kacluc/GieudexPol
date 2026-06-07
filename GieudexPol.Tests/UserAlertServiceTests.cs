using Xunit;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GieudexPol.Application.Interfaces;
using GieudexPol.Application.Services;
using GieudexPol.Domain.Entities;
using System;

namespace GieudexPol.Tests
{
    public class UserAlertServiceTests
    {
        private readonly Mock<IUserAlertRepository> _mockUserAlertRepository;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly UserAlertService _userAlertService;

        public UserAlertServiceTests()
        {
            _mockUserAlertRepository = new Mock<IUserAlertRepository>();
            _mockNotificationService = new Mock<INotificationService>();
            _userAlertService = new UserAlertService(
                _mockUserAlertRepository.Object,
                _mockNotificationService.Object
            );
        }

        [Fact]
        public async Task GetUserAlertsByUserIdAsync_ShouldReturnUserAlerts()
        {
            // Arrange
            int userId = 1;
            var userAlerts = new List<UserAlert>
            {
                new UserAlert { Id = 1, UserId = userId, CurrencyId = 1, AlertType = AlertType.PriceDrop, IsActive = true, CreatedDate = DateTime.UtcNow },
                new UserAlert { Id = 2, UserId = userId, CurrencyId = 2, AlertType = AlertType.PriceIncrease, IsActive = true, CreatedDate = DateTime.UtcNow }
            };
            _mockUserAlertRepository.Setup(r => r.GetUserAlertsByUserIdAsync(userId))
                                    .ReturnsAsync(userAlerts);

            // Act
            var result = await _userAlertService.GetUserAlertsByUserIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _mockUserAlertRepository.Verify(r => r.GetUserAlertsByUserIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task CreateUserAlertAsync_ShouldAddUserAlertAndSetDefaults()
        {
            // Arrange
            var userAlert = new UserAlert { UserId = 1, CurrencyId = 1, AlertType = AlertType.Threshold, ThresholdValue = 1.2M };

            // Act
            await _userAlertService.CreateUserAlertAsync(userAlert);

            // Assert
            _mockUserAlertRepository.Verify(r => r.AddAsync(It.Is<UserAlert>(ua => ua.UserId == 1 && ua.IsActive == true && ua.CreatedDate != default)), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAlertAsync_ShouldUpdateUserAlert()
        {
            // Arrange
            var userAlert = new UserAlert { Id = 1, UserId = 1, CurrencyId = 1, AlertType = AlertType.PriceDrop, IsActive = true, CreatedDate = DateTime.UtcNow };

            // Act
            await _userAlertService.UpdateUserAlertAsync(userAlert);

            // Assert
            _mockUserAlertRepository.Verify(r => r.UpdateAsync(userAlert), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAlertAsync_ShouldDeleteUserAlertIfExists()
        {
            // Arrange
            int alertId = 1;
            var userAlert = new UserAlert { Id = alertId };
            _mockUserAlertRepository.Setup(r => r.GetByIdAsync(alertId)).ReturnsAsync(userAlert);

            // Act
            await _userAlertService.DeleteUserAlertAsync(alertId);

            // Assert
            _mockUserAlertRepository.Verify(r => r.DeleteAsync(userAlert), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAlertAsync_ShouldDoNothingIfUserAlertDoesNotExist()
        {
            // Arrange
            int alertId = 1;
            _mockUserAlertRepository.Setup(r => r.GetByIdAsync(alertId)).ReturnsAsync((UserAlert?)null);

            // Act
            await _userAlertService.DeleteUserAlertAsync(alertId);

            // Assert
            _mockUserAlertRepository.Verify(r => r.DeleteAsync(It.IsAny<UserAlert>()), Times.Never);
        }

        [Fact]
        public async Task TriggerAlertAsync_ShouldDeactivateAlertAndCreateNotification()
        {
            // Arrange
            int alertId = 1;
            int userId = 1;
            string message = "Test Alert Triggered";
            var userAlert = new UserAlert { Id = alertId, UserId = userId, IsActive = true };
            _mockUserAlertRepository.Setup(r => r.GetByIdAsync(alertId)).ReturnsAsync(userAlert);

            // Act
            await _userAlertService.TriggerAlertAsync(alertId, message);

            // Assert
            _mockUserAlertRepository.Verify(r => r.UpdateAsync(It.Is<UserAlert>(ua => ua.Id == alertId && ua.IsActive == false && ua.TriggeredDate != null)), Times.Once);
            _mockNotificationService.Verify(s => s.AddAsync(It.Is<Notification>(n => n.UserId == userId && n.Message == message && n.IsRead == false)), Times.Once);
        }

        [Fact]
        public async Task TriggerAlertAsync_ShouldDoNothingIfUserAlertDoesNotExist()
        {
            // Arrange
            int alertId = 1;
            string message = "Test Alert Triggered";
            _mockUserAlertRepository.Setup(r => r.GetByIdAsync(alertId)).ReturnsAsync((UserAlert?)null);

            // Act
            await _userAlertService.TriggerAlertAsync(alertId, message);

            // Assert
            _mockUserAlertRepository.Verify(r => r.UpdateAsync(It.IsAny<UserAlert>()), Times.Never);
            _mockNotificationService.Verify(s => s.AddAsync(It.IsAny<Notification>()), Times.Never);
        }
    }
}
