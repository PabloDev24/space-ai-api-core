using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSpaces.Application.Features.Users.Commands.CreateAdmin;
using SmartSpaces.Application.Features.Users.Commands.UpdateUserStatus;
using SmartSpaces.Application.Features.Users.Queries.GetUserById;
using SmartSpaces.Application.Features.Users.Queries.GetUsers;

namespace SmartSpaces.API.Controllers;

[ApiController]
[Route("api/[controller]")] // Esto mapea a: /api/users
[Authorize(Roles = "admin")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet] // GET: /api/users?role=&search=
    public async Task<IActionResult> GetUsers([FromQuery] string? role, [FromQuery] string? search)
    {
        try
        {
            var result = await _mediator.Send(new GetUsersQuery(role, search));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id:guid}")] // GET: /api/users/{id}
    public async Task<IActionResult> GetUserById([FromRoute] Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetUserByIdQuery(id));
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

    [HttpPost("admin")] // POST: /api/users/admin
    public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetUserById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPatch("{id:guid}/status")] // PATCH: /api/users/{id}/status
    public async Task<IActionResult> UpdateStatus([FromRoute] Guid id, [FromBody] UpdateUserStatusRequest request)
    {
        try
        {
            var result = await _mediator.Send(new UpdateUserStatusCommand(id, request.Status));
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

public record UpdateUserStatusRequest(string Status);
