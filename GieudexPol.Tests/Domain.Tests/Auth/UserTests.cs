using GieudexPol.Domain.Auth;
using System;
using Xunit;

namespace GieudexPol.Tests.Domain.Tests.Auth
{
    public class UserTests
    {
        [Fact]
        public void User_Constructor_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var id = Guid.NewGuid();
            var email = "test@example.com";
            var hashedPassword = "hashedpassword123";
            var applicationUserId = 7;
            var displayName = "Test User";

            // Act
            var user = new User(
                id,
                email,
                hashedPassword,
                applicationUserId,
                displayName: displayName);

            // Assert
            Assert.Equal(id, user.Id);
            Assert.Equal(email, user.Email);
            Assert.Equal(hashedPassword, user.HashedPassword);
            Assert.Equal(applicationUserId, user.ApplicationUserId);
            Assert.Equal(displayName, user.DisplayName);
        }

        [Fact]
        public void UpdateEmail_ShouldChangeEmailAddress()
        {
            // Arrange
            var user = new User(Guid.NewGuid(), "old@example.com", "hashedpassword");
            var newEmail = "new@example.com";

            // Act
            user.UpdateEmail(newEmail);

            // Assert
            Assert.Equal(newEmail, user.Email);
        }

        [Fact]
        public void UpdatePassword_ShouldChangeHashedPassword()
        {
            // Arrange
            var user = new User(Guid.NewGuid(), "test@example.com", "oldhashedpassword");
            var newHashedPassword = "newhashedpassword";

            // Act
            user.UpdatePassword(newHashedPassword);

            // Assert
            Assert.Equal(newHashedPassword, user.HashedPassword);
        }
    }
}
