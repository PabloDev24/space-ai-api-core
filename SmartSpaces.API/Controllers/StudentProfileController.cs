using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using SmartSpaces.Application.Features.Users.Queries.GetMyProfile;
using SmartSpaces.Application.Features.Users.Commands.UpdateProfile;
using SmartSpaces.Application.Features.Users.Commands.ChangePassword;

namespace SmartSpaces.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class StudentProfileController : ControllerBase
{
    private readonly IMediator _mediator;
    public StudentProfileController(IMediator mediator) => _mediator = mediator;

    private Guid CurrentUserId => Guid.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
        => Ok(await _mediator.Send(new GetMyProfileQuery(CurrentUserId)));

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileBody body)
    {
        await _mediator.Send(new UpdateProfileCommand(CurrentUserId, body.Telefono, body.EmailAlterno));
        return NoContent();
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordBody body)
    {
        await _mediator.Send(new ChangePasswordCommand(CurrentUserId, body.CurrentPassword, body.NewPassword));
        return NoContent();
    }
}

public record UpdateProfileBody(string? Telefono, string? EmailAlterno);
public record ChangePasswordBody(string CurrentPassword, string NewPassword);