namespace GieudexPol.Application.Exceptions
{
    public class DevelopmentRateSourceNotFoundException : InvalidOperationException
    {
        public DevelopmentRateSourceNotFoundException(string message)
            : base(message)
        {
        }
    }

    public class TestExchangeRateConflictException : InvalidOperationException
    {
        public TestExchangeRateConflictException(string message)
            : base(message)
        {
        }

        public TestExchangeRateConflictException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public class ProtectedExchangeRateException : InvalidOperationException
    {
        public ProtectedExchangeRateException(string message)
            : base(message)
        {
        }
    }
}
