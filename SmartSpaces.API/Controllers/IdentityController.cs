using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSpaces.Application.Features.Identity.Queries.GetActiveSessions;
using SmartSpaces.Application.Features.Identity.Queries.GetActivity;
using SmartSpaces.Application.Features.Identity.Queries.GetMetrics;
using SmartSpaces.Application.Features.Identity.Queries.GetVolume;

namespace SmartSpaces.API.Controllers;

/// <summary>Métricas de identidad y sesiones que consume la vista "Identidad Digital".</summary>
[ApiController]
[Route("api/[controller]")] // Esto mapea a: /api/identity
[Authorize(Roles = "admin")]
public class IdentityController : ControllerBase
{
    private readonly IMediator _mediator;

    public IdentityController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("metrics")] // GET: /api/identity/metrics
    public async Task<IActionResult> GetMetrics()
    {
        try
        {
            var result = await _mediator.Send(new GetIdentityMetricsQuery());
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("activity")] // GET: /api/identity/activity?limit=
    public async Task<IActionResult> GetActivity([FromQuery] int limit = 20)
    {
        try
        {
            var result = await _mediator.Send(new GetIdentityActivityQuery(limit));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("sessions/active")] // GET: /api/identity/sessions/active
    public async Task<IActionResult> GetActiveSessions()
    {
        try
        {
            var result = await _mediator.Send(new GetActiveSessionsQuery());
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("volume")] // GET: /api/identity/volume?period=today|7d|30d|90d
    public async Task<IActionResult> GetVolume([FromQuery] string? period)
    {
        try
        {
            var result = await _mediator.Send(new GetAuthVolumeQuery(period));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
