using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSpaces.Application.Features.Devices.Commands.CreateDevice;
using SmartSpaces.Application.Features.Devices.Commands.DeleteDevice;
using SmartSpaces.Application.Features.Devices.Commands.UpdateDevice;
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

    [HttpPost] // POST: /api/devices
    public async Task<IActionResult> CreateDevice([FromBody] CreateDeviceCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetDeviceById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")] // PUT: /api/devices/{id}
    public async Task<IActionResult> UpdateDevice([FromRoute] Guid id, [FromBody] UpdateDeviceRequest request)
    {
        try
        {
            var command = new UpdateDeviceCommand(id, request.Code, request.Name, request.Type, request.Location, request.Status);
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

    [HttpDelete("{id:guid}")] // DELETE: /api/devices/{id}
    public async Task<IActionResult> DeleteDevice([FromRoute] Guid id)
    {
        try
        {
            await _mediator.Send(new DeleteDeviceCommand(id));
            return NoContent();
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

// El id viaja en la ruta, no en el body: así el PUT no puede recibir dos ids distintos.
public record UpdateDeviceRequest(string Code, string Name, string Type, string? Location, string? Status);
