using MediatR;
using Microsoft.AspNetCore.Mvc;
using TransactionApi.Application.Transactions.Commands.CreateTransaction;
using TransactionApi.Application.Transactions.Queries.GetTransaction;

namespace TransactionApi.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransactionsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Cria uma nova transação financeira.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateTransactionResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTransactionCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.TransactionId }, result);
    }

    /// <summary>Consulta uma transação pelo ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var transaction = await _mediator.Send(new GetTransactionQuery(id), cancellationToken);
        return transaction is null ? NotFound() : Ok(transaction);
    }
}
