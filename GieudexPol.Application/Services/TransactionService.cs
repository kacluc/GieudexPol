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
        private readonly ITransactionFeeRepository _transactionFeeRepository;

        public TransactionService(
            ITransactionRepository transactionRepository,
            IUserRepository userRepository,
            IWalletRepository walletRepository,
            ITransactionFeeRepository transactionFeeRepository)
        {
            _transactionRepository = transactionRepository;
            _userRepository = userRepository;
            _walletRepository = walletRepository;
            _transactionFeeRepository = transactionFeeRepository;
        }

        public async Task<Transaction?> GetByIdAsync(int id)
        {
            return await _transactionRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Transaction>> GetAllAsync()
        {
            // This method might need to be reconsidered if getting all transactions across all users is not desired or performant.
            // For now, it will return an empty list or throw an exception.
            // Alternatively, you could implement a repository method to fetch all transactions without user filtering.
            return Enumerable.Empty<Transaction>(); // Or throw new NotImplementedException("GetAllAsync for transactions is not implemented for all users.");
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

        public async Task<Transaction> CreateTransfer(TransferRequest request)
        {
            var transactionFee = await _transactionFeeRepository.GetActiveTransactionFeeByTypeAsync("Transfer");
            if (transactionFee == null)
            {
                throw new InvalidOperationException("No active transaction fee found for transfers.");
            }

            var sender = await _userRepository.GetByIdAsync(request.SenderId);
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

            if (sender.Id == receiver.Id)
            {
                throw new ArgumentException("Cannot transfer money to yourself.");
            }

            // Calculate the fee
            decimal calculatedFee = request.Amount * (transactionFee.FeePercentage / 100) + transactionFee.FlatFee;
            decimal totalAmountToDeduct = request.Amount + calculatedFee;

            var senderWallet = await _walletRepository.GetUserWalletAsync(request.SenderId, request.CurrencyId);
            if (senderWallet == null || senderWallet.Balance < totalAmountToDeduct)
            {
                throw new InvalidOperationException("Insufficient funds or wallet not found for sender.");
            }

            var receiverWallet = await _walletRepository.GetUserWalletAsync(receiver.Id, request.CurrencyId);
            if (receiverWallet == null)
            {
                // Create receiver wallet if it doesn't exist for the currency
                receiverWallet = new Wallet
                {
                    UserId = receiver.Id,
                    CurrencyId = request.CurrencyId,
                    Balance = 0m // Initialize with 0
                };
                await _walletRepository.AddAsync(receiverWallet);
            }

            var transaction = new Transaction
            {
                SenderId = request.SenderId,
                ReceiverId = receiver.Id,
                Amount = request.Amount,
                CurrencyId = request.CurrencyId,
                Timestamp = DateTime.UtcNow,
                Status = "Pending",
                TransactionType = "Transfer",
                AppliedFee = calculatedFee,
                TransactionFeeId = transactionFee.Id
            };

            await _transactionRepository.AddAsync(transaction);

            try
            {
                // Deduct from sender's wallet (amount + fee)
                senderWallet.Balance -= totalAmountToDeduct;
                await _walletRepository.UpdateAsync(senderWallet);

                // Add to receiver's wallet (only amount)
                receiverWallet.Balance += request.Amount;
                await _walletRepository.UpdateAsync(receiverWallet);

                transaction.Status = "Completed";
                await _transactionRepository.UpdateAsync(transaction);
            }
            catch (Exception ex)
            {
                transaction.Status = "Failed";
                await _transactionRepository.UpdateAsync(transaction);
                // Log the exception
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
                CurrencySymbol = t.Currency?.Symbol ?? "N/A",
                Status = t.Status,
                TransactionType = t.TransactionType,
                AppliedFee = t.AppliedFee,
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
