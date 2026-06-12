using FluentAssertions;
using GieudexPol.Domain.Entities;
using GieudexPol.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace GieudexPol.Tests;

public class ApplicationDbContextModelTests
{
    [Fact]
    public void AuditLog_UserRelationship_UsesAuthIdWithoutShadowForeignKey()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        var auditLogEntity = context.Model.FindEntityType(typeof(AuditLog));

        auditLogEntity.Should().NotBeNull();
        auditLogEntity!.FindProperty("UserId1").Should().BeNull();

        var userForeignKey = auditLogEntity.GetForeignKeys()
            .Single(foreignKey => foreignKey.PrincipalEntityType.ClrType == typeof(User));

        userForeignKey.Properties.Single().Name.Should().Be(nameof(AuditLog.UserId));
        userForeignKey.PrincipalKey.Properties.Single().Name.Should().Be(nameof(User.AuthId));
    }
}
