using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSpaces.Application.Features.Knowledge.Commands.Ask;

namespace SmartSpaces.API.Controllers;

[ApiController]
[Route("api/[controller]")] // Esto mapea a: /api/knowledge
[Authorize]
public class KnowledgeController : ControllerBase
{
    private readonly IMediator _mediator;

    public KnowledgeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("ask")] // POST: /api/knowledge/ask
    public async Task<IActionResult> Ask([FromBody] AskKnowledgeCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
