using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using SmartSpaces.Application.Features.Academic.Queries.GetSummary;
using SmartSpaces.Application.Features.Academic.Queries.GetGrades;
using SmartSpaces.Application.Features.Academic.Queries.GetSchedule;

namespace SmartSpaces.API.Controllers;

[ApiController]
[Route("api/academic")]
[Authorize]
public class AcademicController : ControllerBase
{
    private readonly IMediator _mediator;
    public AcademicController(IMediator mediator) => _mediator = mediator;
    private Guid CurrentUserId => Guid.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary() => Ok(await _mediator.Send(new GetAcademicSummaryQuery(CurrentUserId)));

    [HttpGet("grades")]
    public async Task<IActionResult> GetGrades() => Ok(await _mediator.Send(new GetGradesQuery(CurrentUserId)));

    [HttpGet("schedule")]
    public async Task<IActionResult> GetSchedule() => Ok(await _mediator.Send(new GetScheduleQuery(CurrentUserId)));
}