using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSpaces.Application.Features.Access.Commands.Scan;
using SmartSpaces.Application.Features.Access.Queries.GetLog;

namespace SmartSpaces.API.Controllers;

[ApiController]
[Route("api/[controller]")] // Esto mapea a: /api/access
[Authorize]
public class AccessController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccessController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("scan")] // POST: /api/access/scan
    public async Task<IActionResult> Scan([FromBody] ScanAccessCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("log")] // GET: /api/access/log
    public async Task<IActionResult> GetLog([FromQuery] int limit = 50)
    {
        try
        {
            var result = await _mediator.Send(new GetAccessLogQuery(limit));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
