using System.Collections.Generic;

namespace GieudexPol.Domain.Entities
{
    public class RateSource
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int? SystemUserId { get; set; }
        public User? SystemUser { get; set; }
        public ICollection<ExchangeRate> ExchangeRates { get; set; } = new List<ExchangeRate>();
        public ICollection<UserAlert> UserAlerts { get; set; } = new List<UserAlert>();
        public ICollection<UserAlertEvaluationState> AlertEvaluationStates { get; set; } =
            new List<UserAlertEvaluationState>();
        public ICollection<ExchangeExecution> ExchangeExecutions { get; set; } =
            new List<ExchangeExecution>();
    }
}
