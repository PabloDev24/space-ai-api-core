using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSpaces.Application.Features.Devices.Queries.GetDeviceById;
using SmartSpaces.Application.Features.Devices.Queries.GetDevices;

namespace SmartSpaces.API.Controllers;

[ApiController]
[Route("api/[controller]")] // Esto mapea a: /api/devices
[Authorize(Roles = "admin")]
public class DevicesController : ControllerBase
{
    private readonly IMediator _mediator;

    public DevicesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet] // GET: /api/devices?type=&status=
    public async Task<IActionResult> GetDevices([FromQuery] string? type, [FromQuery] string? status)
    {
        try
        {
            var result = await _mediator.Send(new GetDevicesQuery(type, status));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id:guid}")] // GET: /api/devices/{id}
    public async Task<IActionResult> GetDeviceById([FromRoute] Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetDeviceByIdQuery(id));
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
