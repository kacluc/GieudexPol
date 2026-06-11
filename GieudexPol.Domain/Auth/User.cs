namespace GieudexPol.Domain.Auth
{
    public class User
    {
        public Guid Id { get; private set; }
        public int ApplicationUserId { get; private set; }
        public string Email { get; private set; } = string.Empty;
        public string DisplayName { get; private set; } = string.Empty;
        public string HashedPassword { get; private set; } = string.Empty;
        public string Role { get; private set; } = "User";

        private User() { }

        public User(
            Guid id,
            string email,
            string hashedPassword,
            int applicationUserId = 0,
            string role = "User",
            string displayName = "")
        {
            Id = id;
            ApplicationUserId = applicationUserId;
            Email = email;
            DisplayName = displayName;
            HashedPassword = hashedPassword;
            Role = role;
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

        public void UpdateDisplayName(string displayName)
        {
            DisplayName = displayName;
        }
    }
}
