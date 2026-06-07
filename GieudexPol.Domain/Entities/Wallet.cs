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

        /// <summary>
        /// Zmniejsza saldo portfela o podaną kwotę.
        /// </summary>
        public void Debit(decimal amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Kwota obciążenia musi być większa od zera.", nameof(amount));
            }

            if (Balance < amount)
            {
                throw new InvalidOperationException("Niewystarczające środki na koncie portfela.");
            }

            Balance -= amount;
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
