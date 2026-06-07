namespace GieudexPol.Domain.Auth
{
    public class User
    {
        public Guid Id { get; private set; }
        public int ApplicationUserId { get; private set; }
        public string Email { get; private set; } = string.Empty;
        public string HashedPassword { get; private set; } = string.Empty;

        private User() { }

        public User(Guid id, string email, string hashedPassword, int applicationUserId = 0)
        {
            Id = id;
            ApplicationUserId = applicationUserId;
            Email = email;
            HashedPassword = hashedPassword;
        }

        public void AssignApplicationUserId(int applicationUserId)
        {
            ApplicationUserId = applicationUserId;
        }

        public void UpdateEmail(string newEmail)
        {
            Email = newEmail;
        }

        public void UpdatePassword(string newHashedPassword)
        {
            HashedPassword = newHashedPassword;
        }
    }
}
