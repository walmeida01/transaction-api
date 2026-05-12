using FluentAssertions;
using FluentValidation;
using TransactionApi.Application.Common.Behaviors;
using TransactionApi.Application.Transactions.Commands.CreateTransaction;

namespace TransactionApi.UnitTests.Application;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_InvalidRequest_ShouldThrowValidationException()
    {
        // Arrange
        var validators = new List<IValidator<CreateTransactionCommand>>
        {
            new CreateTransactionValidator()
        };
        var behavior = new ValidationBehavior<CreateTransactionCommand, CreateTransactionResult>(validators);

        var invalidCommand = new CreateTransactionCommand(
            SourceAccountId: "",   // vazio — deve falhar
            DestinationAccountId: "acc-002",
            Amount: -100,          // negativo — deve falhar
            Currency: "XX");       // inválido — deve falhar

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            behavior.Handle(
                invalidCommand,
                () => Task.FromResult(default(CreateTransactionResult)!),
                CancellationToken.None));

        ex.Errors.Should().Contain(e => e.PropertyName == "SourceAccountId");
        ex.Errors.Should().Contain(e => e.PropertyName == "Amount");
        ex.Errors.Should().Contain(e => e.PropertyName == "Currency");
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldCallNext()
    {
        // Arrange
        var validators = new List<IValidator<CreateTransactionCommand>>
        {
            new CreateTransactionValidator()
        };
        var behavior = new ValidationBehavior<CreateTransactionCommand, CreateTransactionResult>(validators);
        var nextCalled = false;

        var validCommand = new CreateTransactionCommand(
            SourceAccountId: "acc-001",
            DestinationAccountId: "acc-002",
            Amount: 100m,
            Currency: "BRL");

        // Act
        await behavior.Handle(
            validCommand,
            () => { nextCalled = true; return Task.FromResult(new CreateTransactionResult(Guid.NewGuid(), "Pending")); },
            CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
    }
}
