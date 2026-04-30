using FinTrack.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(ApiResponse<object>.Ok(new { application = "FinTrack", status = "Healthy" }, "Service is healthy."));
    }
}
