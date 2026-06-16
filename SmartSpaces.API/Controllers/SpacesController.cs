using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartSpaces.Application.Features.Auth.Commands;
using Microsoft.AspNetCore.Authorization;
using SmartSpaces.Application.Features.Auth.Queries;
using SmartSpaces.Application.Features.Auth.Queries.GetQrToken;
using SmartSpaces.Application.Features.Auth.Commands.ValidateQr;

namespace SmartSpaces.API.Controllers;

[ApiController]
[Route("api/[controller]")] // Esto mapea a: /api/auth
[Authorize]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")] // POST: /api/auth/register
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            // Retorna un 201 Created
            return StatusCode(201, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("login")] // POST: /api/auth/login
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginQuery query)
    {
        try
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            // Retorna 401 Unauthorized exacto al contrato si la contraseña falla
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("qr/{userId:guid}")] // GET: /api/auth/qr/{userId}
    public async Task<IActionResult> GetQrToken([FromRoute] Guid userId)
    {
        try
        {
            var query = new GetQrTokenQuery(userId);
            var result = await _mediator.Send(query);
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

    [HttpPost("qr/validate")] // POST: /api/auth/qr/validate
    [AllowAnonymous]
    public async Task<IActionResult> ValidateQr([FromBody] ValidateQrCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            if (result.IsValid)
            {
                return Ok(result);
            }
            else
            {
                return Unauthorized(new { error = result.Message });
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}