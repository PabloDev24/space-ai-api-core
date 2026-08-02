using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSpaces.Application.Features.Access.Commands.CreatePoint;
using SmartSpaces.Application.Features.Access.Queries.GetPoints;

namespace SmartSpaces.API.Controllers;

/// <summary>
/// Gestión de los puntos de acceso físicos (plumas, torniquetes, puertas).
/// Va en su propio controlador para no exponer /scan y /log bajo el prefijo access-control.
/// </summary>
[ApiController]
[Route("api/access")]         // Ruta del contrato: /api/access/points
[Route("api/access-control")] // Alias: es la que ya consume access-control.service.ts del panel
[Authorize(Roles = "admin")]
public class AccessPointsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccessPointsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("points")] // GET: /api/access/points
    public async Task<IActionResult> GetPoints()
    {
        try
        {
            var result = await _mediator.Send(new GetAccessPointsQuery());
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("points")] // POST: /api/access/points
    public async Task<IActionResult> CreatePoint([FromBody] CreateAccessPointCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return StatusCode(201, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
