using FluentAssertions;
using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using GieudexPol.Application.Services;
using GieudexPol.Domain.Entities;
using Moq;
using Xunit;

namespace GieudexPol.Tests
{
    public class WalletServiceTests
    {
        private readonly Mock<IWalletRepository> _walletRepository = new();
        private readonly Mock<ICurrencyService> _currencyService = new();
        private readonly Mock<ITransactionFeeCalculator> _transactionFeeCalculator = new();
        private readonly Mock<IInstantExchangeService> _instantExchangeService = new();
        private readonly WalletService _walletService;

        public WalletServiceTests()
        {
            _walletService = new WalletService(
                _walletRepository.Object,
                _currencyService.Object,
                _transactionFeeCalculator.Object,
                _instantExchangeService.Object);
        }

        [Fact]
        public async Task AddAsync_ShouldCallRepositoryAddAsync()
        {
            var wallet = new Wallet { UserId = 1, CurrencyId = 1, Balance = 100m };

            await _walletService.AddAsync(wallet);

            _walletRepository.Verify(repository => repository.AddAsync(wallet), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnWallet_WhenWalletExists()
        {
            var expectedWallet = new Wallet { Id = 1, UserId = 1, CurrencyId = 1, Balance = 100m };
            _walletRepository.Setup(repository => repository.GetByIdAsync(1)).ReturnsAsync(expectedWallet);

            var result = await _walletService.GetByIdAsync(1);

            result.Should().BeEquivalentTo(expectedWallet);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenWalletDoesNotExist()
        {
            _walletRepository.Setup(repository => repository.GetByIdAsync(1)).ReturnsAsync((Wallet?)null);

            var result = await _walletService.GetByIdAsync(1);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllWallets()
        {
            var expectedWallets = new List<Wallet>
            {
                new Wallet { Id = 1, UserId = 1, CurrencyId = 1, Balance = 100m },
                new Wallet { Id = 2, UserId = 2, CurrencyId = 2, Balance = 50m }
            };
            _walletRepository.Setup(repository => repository.GetAllAsync()).ReturnsAsync(expectedWallets);

            var result = await _walletService.GetAllAsync();

            result.Should().BeEquivalentTo(expectedWallets);
        }

        [Fact]
        public async Task UpdateAsync_ShouldCallRepositoryUpdateAsync()
        {
            var wallet = new Wallet { Id = 1, UserId = 1, CurrencyId = 1, Balance = 150m };

            await _walletService.UpdateAsync(wallet);

            _walletRepository.Verify(repository => repository.UpdateAsync(wallet), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallRepositoryDeleteAsync()
        {
            var wallet = new Wallet { Id = 1, UserId = 1, CurrencyId = 1, Balance = 100m };

            await _walletService.DeleteAsync(wallet);

            _walletRepository.Verify(repository => repository.DeleteAsync(wallet), Times.Once);
        }

        [Fact]
        public async Task GetUserWalletsAsync_ShouldReturnWallets_WhenUserHasWallets()
        {
            var expectedWallets = new List<Wallet>
            {
                new Wallet { Id = 1, UserId = 1, CurrencyId = 1, Balance = 100m },
                new Wallet { Id = 2, UserId = 1, CurrencyId = 2, Balance = 50m }
            };
            _walletRepository.Setup(repository => repository.GetUserWalletsAsync(1)).ReturnsAsync(expectedWallets);

            var result = await _walletService.GetUserWalletsAsync(1);

            result.Should().BeEquivalentTo(expectedWallets);
        }

        [Fact]
        public async Task GetUserWalletsAsync_ShouldReturnEmptyList_WhenUserHasNoWallets()
        {
            _walletRepository.Setup(repository => repository.GetUserWalletsAsync(1)).ReturnsAsync(new List<Wallet>());

            var result = await _walletService.GetUserWalletsAsync(1);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task AddCurrencyWalletAsync_ShouldSaveOnlyCurrencyIdAndReturnCurrencyDetails()
        {
            var currency = new Currency { Id = 3, Symbol = "CHF", IsActive = true };
            _walletRepository.Setup(repository => repository.GetUserWalletAsync(1, currency.Id))
                .ReturnsAsync((Wallet?)null);
            _walletRepository.Setup(repository => repository.GetUserWalletsAsync(1))
                .ReturnsAsync(new List<Wallet>());
            _currencyService.Setup(service => service.GetTradableCurrenciesAsync())
                .ReturnsAsync([currency]);
            Wallet? savedWallet = null;
            _walletRepository.Setup(repository => repository.AddAsync(It.IsAny<Wallet>()))
                .Callback<Wallet>(wallet => savedWallet = new Wallet
                {
                    UserId = wallet.UserId,
                    CurrencyId = wallet.CurrencyId,
                    Balance = wallet.Balance,
                    Currency = wallet.Currency
                })
                .Returns(Task.CompletedTask);

            var result = await _walletService.AddCurrencyWalletAsync(1, currency.Id);

            savedWallet.Should().NotBeNull();
            savedWallet!.UserId.Should().Be(1);
            savedWallet.CurrencyId.Should().Be(currency.Id);
            savedWallet.Balance.Should().Be(0);
            savedWallet.Currency.Should().BeNull();
            result.Currency.Should().BeSameAs(currency);
        }

        [Fact]
        public async Task ExecuteTradeTransactionAsync_ShouldUseLowestSellPriceAcrossSourcesWhenBuying()
        {
            var operationDate = DateTime.Today;
            _instantExchangeService.Setup(service => service.ExecuteAsync(
                    1,
                    1,
                    42m,
                    2,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TradeExecutionResultDto
                {
                    AmountTo = 10m,
                    BuyRateSource = "ECB",
                    SellRateSource = "ECB",
                    EffectiveDate = operationDate
                });

            var result = await _walletService.ExecuteTradeTransactionAsync(1, 1, 42m, 2);

            result.AmountTo.Should().Be(10m);
            result.BuyRateSource.Should().Be("ECB");
            result.SellRateSource.Should().Be("ECB");
            result.EffectiveDate.Should().Be(operationDate);
            _instantExchangeService.Verify(service => service.ExecuteAsync(
                1, 1, 42m, 2, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteTradeTransactionAsync_ShouldUseHighestBuyPriceOnPreviousPublicationDate()
        {
            var previousPublicationDate = DateTime.Today.AddDays(-1);
            _instantExchangeService.Setup(service => service.ExecuteAsync(
                    1,
                    2,
                    2m,
                    1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TradeExecutionResultDto
                {
                    AmountTo = 8.20m,
                    SellRateSource = "BOE",
                    BuyRateSource = "BOE",
                    EffectiveDate = previousPublicationDate
                });

            var result = await _walletService.ExecuteTradeTransactionAsync(1, 2, 2m, 1);

            result.AmountTo.Should().Be(8.20m);
            result.SellRateSource.Should().Be("BOE");
            result.BuyRateSource.Should().Be("BOE");
            result.EffectiveDate.Should().Be(previousPublicationDate);
        }

        [Fact]
        public async Task DepositAsync_CreditsAmountReducedByFeeAndStoresFee()
        {
            var wallet = new Wallet { Id = 10, UserId = 1, CurrencyId = 1, Balance = 50m };
            _walletRepository.Setup(repository => repository.GetUserWalletsAsync(1))
                .ReturnsAsync([wallet]);
            _transactionFeeCalculator.Setup(calculator => calculator.CalculateAsync(
                    "Deposit",
                    wallet.CurrencyId,
                    100m,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new OperationFeeCalculationDto(10m, null));

            await _walletService.DepositAsync(1, wallet.CurrencyId, 100m);

            _walletRepository.Verify(repository => repository.ExecuteBalanceOperationAsync(
                wallet.Id,
                90m,
                It.Is<Transaction>(transaction =>
                    transaction.Amount == 100m &&
                    transaction.AppliedFee == 10m &&
                    transaction.TransactionType == "Deposit")), Times.Once);
        }

        [Fact]
        public async Task WithdrawAsync_DebitsAmountAndFeeAndStoresFee()
        {
            var wallet = new Wallet { Id = 10, UserId = 1, CurrencyId = 1, Balance = 150m };
            _walletRepository.Setup(repository => repository.GetUserWalletsAsync(1))
                .ReturnsAsync([wallet]);
            _transactionFeeCalculator.Setup(calculator => calculator.CalculateAsync(
                    "Withdrawal",
                    wallet.CurrencyId,
                    100m,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new OperationFeeCalculationDto(10m, null));

            await _walletService.WithdrawAsync(1, wallet.CurrencyId, 100m);

            _walletRepository.Verify(repository => repository.ExecuteBalanceOperationAsync(
                wallet.Id,
                -110m,
                It.Is<Transaction>(transaction =>
                    transaction.Amount == 100m &&
                    transaction.AppliedFee == 10m &&
                    transaction.TransactionType == "Withdrawal")), Times.Once);
        }
    }
}
