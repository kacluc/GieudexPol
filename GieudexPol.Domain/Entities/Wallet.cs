using System;

namespace GieudexPol.Domain.Entities
{
    public class Wallet
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int CurrencyId { get; set; }
        public Currency Currency { get; set; } = null!;
        public decimal Balance { get; set; }
        public decimal ReservedBalance { get; set; }
        public decimal AvailableBalance => Balance - ReservedBalance;

        /// <summary>
        /// Zmniejsza saldo portfela o podaną kwotę.
        /// </summary>
        public void Debit(decimal amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Kwota obciążenia musi być większa od zera.", nameof(amount));
            }

            if (AvailableBalance < amount)
            {
                throw new InvalidOperationException("Niewystarczające środki na koncie portfela.");
            }

            Balance -= amount;
        }

        public void DebitReserved(decimal amount)
        {
            if (amount <= 0 || Balance < amount || ReservedBalance < amount)
            {
                throw new InvalidOperationException("Niewystarczajace zarezerwowane srodki.");
            }

            Balance -= amount;
            ReservedBalance -= amount;
        }

        public void Reserve(decimal amount)
        {
            if (amount <= 0 || AvailableBalance < amount)
            {
                throw new InvalidOperationException("Niewystarczajace dostepne srodki.");
            }

            ReservedBalance += amount;
        }

        public void Release(decimal amount)
        {
            if (amount < 0 || ReservedBalance < amount)
            {
                throw new InvalidOperationException("Nieprawidlowa kwota zwolnienia rezerwacji.");
            }

            ReservedBalance -= amount;
        }

        /// <summary>
        /// Zwiększa saldo portfela o podaną kwotę.
        /// </summary>
        public void Credit(decimal amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Kwota uznania musi być większa od zera.", nameof(amount));
            }

            Balance += amount;
        }
    }
}
