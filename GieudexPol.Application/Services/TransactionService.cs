using GieudexPol.Application.DTOs;
using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GieudexPol.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUserRepository _userRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly ITransactionFeeCalculator _transactionFeeCalculator;

        public TransactionService(
            ITransactionRepository transactionRepository,
            IUserRepository userRepository,
            IWalletRepository walletRepository,
            ITransactionFeeCalculator transactionFeeCalculator)
        {
            _transactionRepository = transactionRepository;
            _userRepository = userRepository;
            _walletRepository = walletRepository;
            _transactionFeeCalculator = transactionFeeCalculator;
        }

        public async Task<Transaction?> GetByIdAsync(int id)
        {
            return await _transactionRepository.GetByIdAsync(id);
        }

        public Task<IEnumerable<Transaction>> GetAllAsync()
        {
            // This method might need to be reconsidered if getting all transactions across all users is not desired or performant.
            // For now, it will return an empty list or throw an exception.
            // Alternatively, you could implement a repository method to fetch all transactions without user filtering.
            return Task.FromResult(Enumerable.Empty<Transaction>()); // Or throw new NotImplementedException("GetAllAsync for transactions is not implemented for all users.");
        }

        public async Task AddAsync(Transaction entity)
        {
            await _transactionRepository.AddAsync(entity);
        }

        public async Task UpdateAsync(Transaction entity)
        {
            await _transactionRepository.UpdateAsync(entity);
        }

        public async Task DeleteAsync(Transaction entity)
        {
            await _transactionRepository.DeleteAsync(entity.Id);
        }

        public async Task<Transaction> CreateTransfer(int senderId, TransferRequest request)
        {
            var sender = await _userRepository.GetByIdAsync(senderId);
            if (sender == null)
            {
                throw new ArgumentException("Sender not found.");
            }

            var receiverUsername = request.ReceiverUsername.Trim();
            var receiver = await _userRepository.GetByUsernameAsync(receiverUsername);
            if (receiver == null)
            {
                throw new ArgumentException("Receiver not found.");
            }

            if (receiver.AccountType is AccountType.RateSourceSystem or
                AccountType.PlatformTreasury)
            {
                throw new ArgumentException(
                    "Nie mozna wykonac zwyklego transferu do konta systemowego.");
            }

            if (sender.Id == receiver.Id)
            {
                throw new ArgumentException("Cannot transfer money to yourself.");
            }

            var fee = await _transactionFeeCalculator.CalculateAsync(
                "Transfer",
                request.CurrencyId,
                request.Amount);
            var calculatedFee = fee.FeeAmount;
            decimal totalAmountToDeduct = request.Amount + calculatedFee;

            var senderWallet = await _walletRepository.GetUserWalletAsync(senderId, request.CurrencyId);
            if (senderWallet == null || senderWallet.AvailableBalance < totalAmountToDeduct)
            {
                throw new InvalidOperationException("Insufficient funds or wallet not found for sender.");
            }

            var transaction = new Transaction
            {
                SenderId = senderId,
                ReceiverId = receiver.Id,
                Amount = request.Amount,
                CurrencyId = request.CurrencyId,
                Timestamp = DateTime.UtcNow,
                Status = "Completed",
                TransactionType = "Transfer",
                AppliedFee = calculatedFee,
                TransactionFeeId = fee.TransactionFeeId
            };

            try
            {
                await _walletRepository.ExecuteTransferAsync(
                    senderWallet.Id,
                    receiver.Id,
                    request.CurrencyId,
                    request.Amount,
                    calculatedFee,
                    transaction);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Transaction failed due to an error.", ex);
            }

            return transaction;
        }

        public async Task<PaginatedResult<TransactionDto>> GetUserTransactions(
            int userId,
            int pageNumber,
            int pageSize,
            string? transactionType,
            int? currencyId,
            DateTime? startDate,
            DateTime? endDate)
        {
            var transactions = await _transactionRepository.GetByUserIdAsync(
                userId, pageNumber, pageSize, transactionType, currencyId, startDate, endDate);
            var totalRecords = await _transactionRepository.GetTotalRecordsByUserIdAsync(
                userId, transactionType, currencyId, startDate, endDate);

            var transactionDtos = transactions.Select(t => new TransactionDto
            {
                Id = t.Id,
                SenderUsername = t.Sender?.Username ?? "N/A",
                ReceiverUsername = t.Receiver?.Username ?? "N/A",
                Amount = t.Amount,
                CurrencyId = t.CurrencyId,
                CurrencySymbol = t.Currency?.Symbol ?? "N/A",
                Status = t.Status,
                TransactionType = t.TransactionType,
                AppliedFee = t.AppliedFee,
                TradeExecutionId = t.TradeExecutionId,
                TradingPair = t.TradeExecution == null
                    ? null
                    : t.TradeExecution.TradingPair.BaseCurrency.Symbol + "/" +
                      t.TradeExecution.TradingPair.QuoteCurrency.Symbol,
                ExecutionPrice = t.TradeExecution?.Price,
                ExecutionAmount = t.TradeExecution?.Amount,
                ExchangeExecutionId = t.ExchangeExecutionId,
                ExchangePair = t.ExchangeExecution == null
                    ? null
                    : t.ExchangeExecution.FromCurrency.Symbol + "/" +
                      t.ExchangeExecution.ToCurrency.Symbol,
                RateSource = t.ExchangeExecution?.RateSource.Code,
                ExchangeRate = t.ExchangeExecution?.Rate,
                FeeCurrency = t.TradeExecution?.FeeCurrency?.Symbol ??
                              t.ExchangeExecution?.FeeCurrency.Symbol ??
                              t.Currency?.Symbol,
                Timestamp = t.Timestamp
            }).ToList();

            return new PaginatedResult<TransactionDto>
            {
                Items = transactionDtos,
                TotalCount = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
