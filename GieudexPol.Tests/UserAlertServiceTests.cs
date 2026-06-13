using Xunit;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GieudexPol.Application.Interfaces;
using GieudexPol.Application.Services;
using GieudexPol.Domain.Entities;
using FluentAssertions;
using System;

namespace GieudexPol.Tests
{
    public class UserAlertServiceTests
    {
        private readonly Mock<IUserAlertRepository> _mockUserAlertRepository;
        private readonly Mock<ICurrencyRepository> _mockCurrencyRepository;
        private readonly Mock<IRateSourceRepository> _mockRateSourceRepository;
        private readonly UserAlertService _userAlertService;

        public UserAlertServiceTests()
        {
            _mockUserAlertRepository = new Mock<IUserAlertRepository>();
            _mockCurrencyRepository = new Mock<ICurrencyRepository>();
            _mockRateSourceRepository = new Mock<IRateSourceRepository>();
            _mockCurrencyRepository
                .Setup(repository => repository.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => new Currency { Id = id, Symbol = "EUR", IsActive = true });
            _mockRateSourceRepository
                .Setup(repository => repository.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => new RateSource
                {
                    Id = id,
                    Code = "MOCK_BANK_A",
                    IsActive = true
                });
            _userAlertService = new UserAlertService(
                _mockUserAlertRepository.Object,
                _mockCurrencyRepository.Object,
                _mockRateSourceRepository.Object
            );
        }

        [Fact]
        public async Task GetUserAlertsByUserIdAsync_ShouldReturnUserAlerts()
        {
            // Arrange
            int userId = 1;
            var userAlerts = new List<UserAlert>
            {
                new UserAlert { Id = 1, UserId = userId, CurrencyId = 1, AlertType = AlertType.PriceDrop, Status = AlertStatus.Active, CreatedDate = DateTime.UtcNow },
                new UserAlert { Id = 2, UserId = userId, CurrencyId = 2, AlertType = AlertType.PriceIncrease, Status = AlertStatus.Active, CreatedDate = DateTime.UtcNow }
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
            var userAlert = new UserAlert
            {
                UserId = 1,
                CurrencyId = 1,
                AlertType = AlertType.Threshold,
                PriceSide = AlertPriceSide.UserBuysCurrency,
                ThresholdValue = 1.2M,
                ThresholdDirection = ThresholdDirection.BelowOrEqual
            };

            // Act
            await _userAlertService.CreateUserAlertAsync(userAlert);

            // Assert
            _mockUserAlertRepository.Verify(r => r.AddAsync(It.Is<UserAlert>(ua =>
                ua.UserId == 1 &&
                ua.Status == AlertStatus.Active &&
                ua.CreatedDate != default)), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAlertAsync_ShouldUpdateUserAlert()
        {
            // Arrange
            var userAlert = new UserAlert
            {
                Id = 1,
                UserId = 1,
                CurrencyId = 1,
                AlertType = AlertType.PriceDrop,
                PriceSide = AlertPriceSide.UserBuysCurrency,
                PercentageChange = 2m,
                Status = AlertStatus.Active,
                CreatedDate = DateTime.UtcNow
            };

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
        public async Task CreateThresholdWithoutValue_ShouldFailValidation()
        {
            var alert = ValidThresholdAlert();
            alert.ThresholdValue = null;

            await Assert.ThrowsAsync<ArgumentException>(
                () => _userAlertService.CreateUserAlertAsync(alert));
        }

        [Fact]
        public async Task CreateThresholdWithoutDirection_ShouldFailValidation()
        {
            var alert = ValidThresholdAlert();
            alert.ThresholdDirection = null;

            await Assert.ThrowsAsync<ArgumentException>(
                () => _userAlertService.CreateUserAlertAsync(alert));
        }

        [Theory]
        [InlineData(AlertType.PriceIncrease)]
        [InlineData(AlertType.PriceDrop)]
        public async Task CreatePercentageAlertWithoutPercentage_ShouldFailValidation(
            AlertType alertType)
        {
            var alert = new UserAlert
            {
                CurrencyId = 1,
                AlertType = alertType,
                PriceSide = AlertPriceSide.UserBuysCurrency
            };

            await Assert.ThrowsAsync<ArgumentException>(
                () => _userAlertService.CreateUserAlertAsync(alert));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task CreatePercentageAlertWithNonPositivePercentage_ShouldFailValidation(
            decimal percentage)
        {
            var alert = new UserAlert
            {
                CurrencyId = 1,
                AlertType = AlertType.PriceIncrease,
                PriceSide = AlertPriceSide.UserBuysCurrency,
                PercentageChange = percentage
            };

            await Assert.ThrowsAsync<ArgumentException>(
                () => _userAlertService.CreateUserAlertAsync(alert));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task CreateThresholdWithNonPositiveValue_ShouldFailValidation(decimal value)
        {
            var alert = ValidThresholdAlert();
            alert.ThresholdValue = value;

            await Assert.ThrowsAsync<ArgumentException>(
                () => _userAlertService.CreateUserAlertAsync(alert));
        }

        [Fact]
        public async Task CreateWithInactiveRateSource_ShouldFailValidation()
        {
            _mockRateSourceRepository
                .Setup(repository => repository.GetByIdAsync(7))
                .ReturnsAsync(new RateSource { Id = 7, IsActive = false });
            var alert = ValidThresholdAlert();
            alert.RateSourceId = 7;

            await Assert.ThrowsAsync<ArgumentException>(
                () => _userAlertService.CreateUserAlertAsync(alert));
        }

        [Fact]
        public async Task NonAdminCannotCreateAlertForMockBank()
        {
            var alert = ValidThresholdAlert();
            alert.RateSourceId = 7;

            var action = () => _userAlertService.CreateUserAlertAsync(alert);

            await action.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*administratorow*");
        }

        [Fact]
        public async Task AdminCanCreateAlertForMockBank()
        {
            var alert = ValidThresholdAlert();
            alert.RateSourceId = 7;

            await _userAlertService.CreateUserAlertAsync(
                alert,
                allowTestRateSources: true);

            _mockUserAlertRepository.Verify(
                repository => repository.AddAsync(alert),
                Times.Once);
        }

        [Fact]
        public async Task NonAdminRateSourceListExcludesMockBanks()
        {
            _mockRateSourceRepository
                .Setup(repository => repository.GetActiveAsync())
                .ReturnsAsync([
                    new RateSource { Id = 1, Code = "NBP", IsActive = true },
                    new RateSource { Id = 2, Code = "MOCK_BANK_A", IsActive = true },
                    new RateSource { Id = 3, Code = "MOCK_BANK_B", IsActive = true }
                ]);

            var result = await _userAlertService.GetActiveRateSourcesAsync();

            result.Select(source => source.Code).Should().Equal("NBP");
        }

        private static UserAlert ValidThresholdAlert()
        {
            return new UserAlert
            {
                CurrencyId = 1,
                AlertType = AlertType.Threshold,
                PriceSide = AlertPriceSide.UserBuysCurrency,
                ThresholdValue = 4.2m,
                ThresholdDirection = ThresholdDirection.BelowOrEqual
            };
        }
    }
}
