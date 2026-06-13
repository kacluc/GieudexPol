namespace GieudexPol.Application.DTOs
{
    public class AdminSystemAccountDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public string? RateSourceCode { get; set; }
        public string? RateSourceName { get; set; }
        public bool? RateSourceIsActive { get; set; }
        public IReadOnlyList<AdminSystemWalletDto> Wallets { get; set; } = [];
    }

    public class AdminSystemWalletDto
    {
        public int CurrencyId { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public string CurrencyName { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public decimal ReservedBalance { get; set; }
        public decimal AvailableBalance { get; set; }
    }
}
