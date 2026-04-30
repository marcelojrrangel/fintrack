using FinTrack.Application.Common.Models;
using FinTrack.Application.Features.Transactions.Commands;
using FinTrack.Application.Features.Transactions.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TransactionsController : ControllerBase
{
    private readonly ISender _sender;

    public TransactionsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<TransactionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<TransactionDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetTransactionsQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<TransactionDto>>.Ok(result, "Transactions retrieved successfully."));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TransactionDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetTransactionByIdQuery(id), cancellationToken);
        return Ok(ApiResponse<TransactionDto>.Ok(result, "Transaction retrieved successfully."));
    }

    [HttpGet("{id:guid}/history")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<TransactionHistoryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<TransactionHistoryDto>>>> GetHistory(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetTransactionHistoryQuery(id), cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<TransactionHistoryDto>>.Ok(result, "Transaction history retrieved successfully."));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TransactionDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<TransactionDto>>> Create(
        [FromBody] TransactionMutationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreateTransactionCommand(request.CategoryId, request.Amount, request.TransactionDateUtc, request.Type, request.Description),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            ApiResponse<TransactionDto>.Ok(result, "Transaction created successfully."));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TransactionDto>>> Update(
        Guid id,
        [FromBody] TransactionMutationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new UpdateTransactionCommand(id, request.CategoryId, request.Amount, request.TransactionDateUtc, request.Type, request.Description),
            cancellationToken);

        return Ok(ApiResponse<TransactionDto>.Ok(result, "Transaction updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TransactionDto>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteTransactionCommand(id), cancellationToken);
        return Ok(ApiResponse<TransactionDto>.Ok(result, "Transaction deleted successfully."));
    }
}
