namespace GieudexPol.Domain.Entities
{
    public class UserAlertEvaluationState
    {
        public int Id { get; set; }
        public int UserAlertId { get; set; }
        public int RateSourceId { get; set; }
        public DateTime LastEvaluatedEffectiveDate { get; set; }

        public UserAlert UserAlert { get; set; } = null!;
        public RateSource RateSource { get; set; } = null!;
    }
}
