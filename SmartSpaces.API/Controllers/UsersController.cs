using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
}
