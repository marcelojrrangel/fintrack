using FinTrack.Application.Common.Models;
using FinTrack.Application.Features.Dashboard.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DashboardController : ControllerBase
{
    private readonly ISender _sender;

    public DashboardController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<DashboardDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DashboardDto>>> Get(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetDashboardQuery(), cancellationToken);
        return Ok(ApiResponse<DashboardDto>.Ok(result, "Dashboard generated successfully."));
    }
}
