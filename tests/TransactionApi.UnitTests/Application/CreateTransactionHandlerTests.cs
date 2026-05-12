using Microsoft.Extensions.Logging;
using Moq;
using TransactionApi.Application.Transactions.Commands.CreateTransaction;
using TransactionApi.Domain.Events;
using TransactionApi.Domain.Interfaces;
using FluentAssertions;
using TransactionApi.Domain.Entities;
using TransactionApi.Domain.Exceptions;

namespace TransactionApi.UnitTests.Application;

public class CreateTransactionHandlerTests
{
    private readonly Mock<ITransactionRepository> _repositoryMock = new();
    private readonly Mock<IEventPublisher> _eventPublisherMock = new();
    private readonly Mock<ILogger<CreateTransactionCommandHandler>> _loggerMock = new();
    private readonly CreateTransactionCommandHandler _handler;

    public CreateTransactionHandlerTests()
    {
        _handler = new CreateTransactionCommandHandler(
            _repositoryMock.Object,
            _eventPublisherMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateTransaction()
    {
        // Arrange
        var command = new CreateTransactionCommand(
            SourceAccountId: "acc-001",
            DestinationAccountId: "acc-002",
            Amount: 100m,
            Currency: "BRL");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.TransactionId.Should().NotBeEmpty();
        result.Status.Should().Be("Pending");

        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _eventPublisherMock.Verify(
            e => e.PublishAsync(It.IsAny<TransactionCreatedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ZeroAmount_ShouldThrowDomainException()
    {
        // Arrange
        var command = new CreateTransactionCommand(
            SourceAccountId: "acc-001",
            DestinationAccountId: "acc-002",
            Amount: 0m,
            Currency: "BRL");

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            () => _handler.Handle(command, CancellationToken.None));

        _eventPublisherMock.Verify(
            e => e.PublishAsync(It.IsAny<TransactionCreatedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
