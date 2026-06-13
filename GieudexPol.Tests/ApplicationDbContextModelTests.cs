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

    [Fact]
    public void Transaction_TradeExecutionRelationship_IsOptionalAndUsesExplicitForeignKey()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        var transactionEntity = context.Model.FindEntityType(typeof(Transaction));

        transactionEntity.Should().NotBeNull();
        var foreignKey = transactionEntity!.GetForeignKeys()
            .Single(candidate =>
                candidate.PrincipalEntityType.ClrType == typeof(TradeExecution));

        foreignKey.Properties.Single().Name.Should().Be(nameof(Transaction.TradeExecutionId));
        foreignKey.IsRequired.Should().BeFalse();
    }

    [Fact]
    public void AlertLog_HasOptionalExplicitRelationshipsToBothAlertTypes()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        var logEntity = context.Model.FindEntityType(typeof(AlertLog));

        logEntity.Should().NotBeNull();
        logEntity!.GetForeignKeys()
            .Select(foreignKey => foreignKey.Properties.Single().Name)
            .Should().BeEquivalentTo(
                nameof(AlertLog.UserAlertId),
                nameof(AlertLog.UserTradingAlertId));
        logEntity.GetForeignKeys().Should().OnlyContain(
            foreignKey => !foreignKey.IsRequired);
    }
}
