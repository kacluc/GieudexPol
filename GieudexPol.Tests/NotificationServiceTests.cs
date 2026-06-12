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
    public class NotificationServiceTests
    {
        private readonly Mock<INotificationRepository> _mockNotificationRepository;
        private readonly NotificationService _notificationService;

        public NotificationServiceTests()
        {
            _mockNotificationRepository = new Mock<INotificationRepository>();
            _notificationService = new NotificationService(_mockNotificationRepository.Object);
        }

        [Fact]
        public async Task GetUserNotificationsAsync_ShouldReturnNotifications()
        {
            // Arrange
            int userId = 1;
            var notifications = new List<Notification>
            {
                new Notification { Id = 1, UserId = userId, Message = "Test Notification 1", CreatedDate = DateTime.UtcNow, IsRead = false },
                new Notification { Id = 2, UserId = userId, Message = "Test Notification 2", CreatedDate = DateTime.UtcNow, IsRead = false }
            };
            _mockNotificationRepository.Setup(r => r.GetUserNotificationsAsync(userId))
                                    .ReturnsAsync(notifications);

            // Act
            var result = await _notificationService.GetUserNotificationsAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _mockNotificationRepository.Verify(r => r.GetUserNotificationsAsync(userId), Times.Once);
        }

        [Fact]
        public async Task MarkNotificationAsReadAsync_ShouldCallRepositoryMethod()
        {
            // Arrange
            int notificationId = 1;

            // Act
            _mockNotificationRepository
                .Setup(repository => repository.MarkNotificationAsReadAsync(notificationId, 2))
                .ReturnsAsync(true);
            var result = await _notificationService.MarkNotificationAsReadAsync(notificationId, 2);

            // Assert
            Assert.True(result);
            _mockNotificationRepository.Verify(
                r => r.MarkNotificationAsReadAsync(notificationId, 2),
                Times.Once);
        }

        [Fact]
        public async Task AddAsync_ShouldCallRepositoryAddAsync()
        {
            // Arrange
            var notification = new Notification { Id = 1, UserId = 1, Message = "Test", CreatedDate = DateTime.UtcNow, IsRead = false };

            // Act
            await _notificationService.AddAsync(notification);

            // Assert
            _mockNotificationRepository.Verify(r => r.AddAsync(notification), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldCallRepositoryUpdateAsync()
        {
            // Arrange
            var notification = new Notification { Id = 1, UserId = 1, Message = "Test", CreatedDate = DateTime.UtcNow, IsRead = false };

            // Act
            await _notificationService.UpdateAsync(notification);

            // Assert
            _mockNotificationRepository.Verify(r => r.UpdateAsync(notification), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallRepositoryDeleteAsync()
        {
            // Arrange
            var notification = new Notification { Id = 1, UserId = 1, Message = "Test", CreatedDate = DateTime.UtcNow, IsRead = false };

            // Act
            await _notificationService.DeleteAsync(notification);

            // Assert
            _mockNotificationRepository.Verify(r => r.DeleteAsync(notification), Times.Once);
        }
    }
}
