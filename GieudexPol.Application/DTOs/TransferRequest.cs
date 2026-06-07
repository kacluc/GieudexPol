using System.ComponentModel.DataAnnotations;

namespace GieudexPol.Application.DTOs
{
    public class TransferRequest
    {
        public int SenderId { get; set; }

        [Required]
        [EmailAddress]
        public string ReceiverUsername { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
        public decimal Amount { get; set; }

        [Range(1, int.MaxValue)]
        public int CurrencyId { get; set; }
    }
}
