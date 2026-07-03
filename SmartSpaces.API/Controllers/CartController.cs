using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSpaces.Application.Features.Cart.Commands.Navigate;
using SmartSpaces.Application.Features.Cart.Commands.Query;
using SmartSpaces.Application.Features.Cart.Queries.GetStatus;

namespace SmartSpaces.API.Controllers;

[ApiController]
[Route("api/[controller]")] // Esto mapea a: /api/cart
[Authorize]
public class CartController : ControllerBase
{
    private readonly IMediator _mediator;

    public CartController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("status")] // GET: /api/cart/status
    public async Task<IActionResult> GetStatus()
    {
        try
        {
            var result = await _mediator.Send(new GetCartStatusQuery());
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("navigate")] // POST: /api/cart/navigate
    public async Task<IActionResult> Navigate([FromBody] NavigateCartCommand command)
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

    [HttpPost("query")] // POST: /api/cart/query
    public async Task<IActionResult> Query([FromBody] CartQueryCommand command)
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
