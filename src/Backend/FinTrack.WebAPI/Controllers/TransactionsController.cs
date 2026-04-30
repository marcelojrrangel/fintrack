using FinTrack.Application.Common.Models;
using FinTrack.Application.Features.Transactions.Commands;
using FinTrack.Application.Features.Transactions.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.WebAPI.Controllers;

/// <summary>
/// Gerenciamento de transações financeiras
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class TransactionsController : ControllerBase
{
    private readonly ISender _sender;

    public TransactionsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Lista todas as transações do usuário com paginação
    /// </summary>
    /// <param name="pageNumber">Número da página (padrão: 1)</param>
    /// <param name="pageSize">Tamanho da página (padrão: 5, máximo: 100)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista paginada de transações</returns>
    /// <response code="200">Transações recuperadas com sucesso</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<TransactionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<TransactionDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 5,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetTransactionsQuery(pageNumber, pageSize), cancellationToken);
        return Ok(ApiResponse<PagedResponse<TransactionDto>>.Ok(result, "Transactions retrieved successfully."));
    }

    /// <summary>
    /// Obtém uma transação específica por ID
    /// </summary>
    /// <param name="id">ID da transação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Detalhes da transação</returns>
    /// <response code="200">Transação encontrada</response>
    /// <response code="404">Transação não encontrada</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TransactionDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetTransactionByIdQuery(id), cancellationToken);
        return Ok(ApiResponse<TransactionDto>.Ok(result, "Transaction retrieved successfully."));
    }

    /// <summary>
    /// Obtém o histórico completo de alterações de uma transação
    /// </summary>
    /// <param name="id">ID da transação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Histórico de alterações da transação</returns>
    /// <response code="200">Histórico recuperado com sucesso</response>
    /// <response code="404">Transação não encontrada</response>
    [HttpGet("{id:guid}/history")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<TransactionHistoryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<TransactionHistoryDto>>>> GetHistory(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetTransactionHistoryQuery(id), cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<TransactionHistoryDto>>.Ok(result, "Transaction history retrieved successfully."));
    }

    /// <summary>
    /// Cria uma nova transação
    /// </summary>
    /// <param name="request">Dados da transação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Transação criada</returns>
    /// <response code="201">Transação criada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TransactionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

    /// <summary>
    /// Atualiza uma transação existente
    /// </summary>
    /// <param name="id">ID da transação</param>
    /// <param name="request">Novos dados da transação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Transação atualizada</returns>
    /// <response code="200">Transação atualizada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="404">Transação não encontrada</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Exclui uma transação (soft delete)
    /// </summary>
    /// <param name="id">ID da transação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Transação excluída</returns>
    /// <response code="200">Transação excluída com sucesso</response>
    /// <response code="404">Transação não encontrada</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TransactionDto>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteTransactionCommand(id), cancellationToken);
        return Ok(ApiResponse<TransactionDto>.Ok(result, "Transaction deleted successfully."));
    }
}
