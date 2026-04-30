using FinTrack.Application.Features.Transactions.Commands;
using FinTrack.Application.Features.Transactions.Queries;
using FinTrack.Domain.Enums;
using FluentAssertions;

namespace FinTrack.Backend.UnitTests.Application;

public sealed class TransactionValidatorTests
{
    [Fact]
    public void CreateValidator_ShouldRejectInvalidCommand_AndAcceptValidCommand()
    {
        var validator = new CreateTransactionCommandValidator();

        var invalid = validator.Validate(new CreateTransactionCommand(Guid.Empty, 0, default, TransactionType.Income, string.Empty));
        var valid = validator.Validate(new CreateTransactionCommand(Guid.NewGuid(), 10, DateTime.UtcNow, TransactionType.Income, "Salary"));

        invalid.IsValid.Should().BeFalse();
        valid.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateValidator_ShouldRejectInvalidTransactionType()
    {
        var validator = new CreateTransactionCommandValidator();

        var invalidType = validator.Validate(new CreateTransactionCommand(
            Guid.NewGuid(), 
            100, 
            DateTime.UtcNow, 
            (TransactionType)0, 
            "Test"));

        invalidType.IsValid.Should().BeFalse();
        invalidType.Errors.Should().Contain(e => e.PropertyName == "Type");
    }

    [Fact]
    public void UpdateValidator_ShouldValidateIdAndPayload()
    {
        var validator = new UpdateTransactionCommandValidator();

        var invalid = validator.Validate(new UpdateTransactionCommand(Guid.Empty, Guid.Empty, 0, default, TransactionType.Income, string.Empty));
        var valid = validator.Validate(new UpdateTransactionCommand(Guid.NewGuid(), Guid.NewGuid(), 10, DateTime.UtcNow, TransactionType.Expense, "Bills"));

        invalid.IsValid.Should().BeFalse();
        valid.IsValid.Should().BeTrue();
    }

    [Fact]
    public void UpdateValidator_ShouldRejectInvalidTransactionType()
    {
        var validator = new UpdateTransactionCommandValidator();

        var invalidType = validator.Validate(new UpdateTransactionCommand(
            Guid.NewGuid(),
            Guid.NewGuid(), 
            100, 
            DateTime.UtcNow, 
            (TransactionType)0, 
            "Test"));

        invalidType.IsValid.Should().BeFalse();
        invalidType.Errors.Should().Contain(e => e.PropertyName == "Type");
    }

    [Fact]
    public void DeleteValidator_ShouldValidateId()
    {
        var validator = new DeleteTransactionCommandValidator();

        validator.Validate(new DeleteTransactionCommand(Guid.Empty)).IsValid.Should().BeFalse();
        validator.Validate(new DeleteTransactionCommand(Guid.NewGuid())).IsValid.Should().BeTrue();
    }

    [Fact]
    public void QueryValidators_ShouldValidateIdentifiers()
    {
        new GetTransactionByIdQueryValidator().Validate(new GetTransactionByIdQuery(Guid.Empty)).IsValid.Should().BeFalse();
        new GetTransactionByIdQueryValidator().Validate(new GetTransactionByIdQuery(Guid.NewGuid())).IsValid.Should().BeTrue();

        new GetTransactionHistoryQueryValidator().Validate(new GetTransactionHistoryQuery(Guid.Empty)).IsValid.Should().BeFalse();
        new GetTransactionHistoryQueryValidator().Validate(new GetTransactionHistoryQuery(Guid.NewGuid())).IsValid.Should().BeTrue();
    }
}
