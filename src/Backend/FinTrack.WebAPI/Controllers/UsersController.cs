using FinTrack.Application.Common.Models;
using FinTrack.Application.Features.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.WebAPI.Controllers;

/// <summary>
/// Gerenciamento de usuários
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class UsersController : ControllerBase
{
    private readonly ISender _sender;

    public UsersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Lista todos os usuários com suporte a busca por nome ou e-mail
    /// </summary>
    /// <param name="search">Termo de busca (nome ou e-mail)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de usuários</returns>
    /// <response code="200">Usuários recuperados com sucesso</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<UserDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<UserDto>>>> GetAll(
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetUsersQuery(search), cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<UserDto>>.Ok(result, "Users retrieved successfully."));
    }
}
