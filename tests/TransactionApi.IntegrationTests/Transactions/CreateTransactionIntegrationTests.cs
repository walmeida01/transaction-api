using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TransactionApi.Application.Transactions.Commands.CreateTransaction;
using TransactionApi.IntegrationTests.Fixtures;

namespace TransactionApi.IntegrationTests.Transactions;

public class CreateTransactionIntegrationTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public CreateTransactionIntegrationTests(ApiFactory factory)
        => _client = factory.CreateClient();

    [Fact]
    public async Task Post_Transaction_WithValidData_Returns201()
    {
        // Arrange
        var payload = new CreateTransactionCommand(
            SourceAccountId: "acc-001",
            DestinationAccountId: "acc-002",
            Amount: 100.00m,
            Currency: "BRL",
            Description: "Integration test");

        // Act
        var response = await _client.PostAsJsonAsync("/transactions", payload);
        var body = await response.Content.ReadFromJsonAsync<CreateTransactionResult>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        body!.TransactionId.Should().NotBeEmpty();
        body.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task Post_Transaction_WithoutAmount_Returns400()
    {
        // Arrange
        var payload = new { sourceAccountId = "acc-001", destinationAccountId = "acc-002", currency = "BRL" };

        // Act
        var response = await _client.PostAsJsonAsync("/transactions", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Transaction_WithNegativeAmount_Returns400()
    {
        // Arrange
        var payload = new CreateTransactionCommand(
            SourceAccountId: "acc-001",
            DestinationAccountId: "acc-002",
            Amount: -50m,
            Currency: "BRL");

        // Act
        var response = await _client.PostAsJsonAsync("/transactions", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
