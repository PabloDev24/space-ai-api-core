using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSpaces.Application.Features.Dashboard.Queries.GetActivity;
using SmartSpaces.Application.Features.Dashboard.Queries.GetSummary;

namespace SmartSpaces.API.Controllers;

[ApiController]
[Route("api/[controller]")] // Esto mapea a: /api/dashboard
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("summary")] // GET: /api/dashboard/summary
    public async Task<IActionResult> GetSummary()
    {
        try
        {
            var result = await _mediator.Send(new GetDashboardSummaryQuery());
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("activity")] // GET: /api/dashboard/activity
    public async Task<IActionResult> GetActivity([FromQuery] int limit = 15)
    {
        try
        {
            var result = await _mediator.Send(new GetDashboardActivityQuery(limit));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
