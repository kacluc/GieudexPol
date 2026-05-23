using GieudexPol.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GieudexPol.Infrastructure
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<ExchangeRate> ExchangeRates { get; set; }
        public DbSet<RateSource> RateSources { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<UserAlert> UserAlerts { get; set; }
        public DbSet<FavoriteCurrency> FavoriteCurrencies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure entity relationships and constraints
            modelBuilder.Entity<User>()
                .HasMany(u => u.Wallets)
                .WithOne(w => w.User)
                .HasForeignKey(w => w.UserId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.UserAlerts)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId);

            modelBuilder.Entity<Wallet>()
                .HasOne(w => w.Currency)
                .WithMany(c => c.Wallets)
                .HasForeignKey(w => w.CurrencyId);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Currency)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.CurrencyId);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.TransactionFee)
                .WithMany()
                .HasForeignKey(t => t.TransactionFeeId);

            modelBuilder.Entity<UserAlert>()
                .HasOne(a => a.Currency)
                .WithMany(c => c.UserAlerts)
                .HasForeignKey(a => a.CurrencyId);

            modelBuilder.Entity<ExchangeRate>()
                .HasOne(er => er.Currency)
                .WithMany(c => c.ExchangeRates)
                .HasForeignKey(er => er.CurrencyId);

            modelBuilder.Entity<ExchangeRate>()
                .HasOne(er => er.RateSource)
                .WithMany(rs => rs.ExchangeRates)
                .HasForeignKey(er => er.RateSourceId);

            modelBuilder.Entity<ExchangeRate>()
                .HasIndex(er => new { er.CurrencyId, er.RateSourceId, er.EffectiveDate })
                .IsUnique();

            modelBuilder.Entity<ExchangeRate>()
                .Property(er => er.BuyPrice)
                .HasPrecision(18, 4);

            modelBuilder.Entity<ExchangeRate>()
                .Property(er => er.SellPrice)
                .HasPrecision(18, 4);

            modelBuilder.Entity<RateSource>()
                .HasIndex(rs => rs.Code)
                .IsUnique();

            modelBuilder.Entity<FavoriteCurrency>()
                .HasIndex(fc => fc.CurrencyCode)
                .IsUnique();
        }
    }
}
