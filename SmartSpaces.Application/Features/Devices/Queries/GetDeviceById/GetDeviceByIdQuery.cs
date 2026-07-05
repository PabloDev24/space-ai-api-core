using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartSpaces.Application.Common.Interfaces;
using SmartSpaces.Application.Features.Devices.Queries.GetDevices;

namespace SmartSpaces.Application.Features.Devices.Queries.GetDeviceById;

public record GetDeviceByIdQuery(Guid Id) : IRequest<DeviceDto>;

public class GetDeviceByIdQueryHandler : IRequestHandler<GetDeviceByIdQuery, DeviceDto>
{
    private readonly IApplicationDbContext _context;

    public GetDeviceByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DeviceDto> Handle(GetDeviceByIdQuery request, CancellationToken cancellationToken)
    {
        var device = await _context.Devices
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (device == null)
        {
            throw new KeyNotFoundException("Dispositivo no encontrado.");
        }

        return new DeviceDto(device.Id, device.Code, device.Name, device.Type, device.Status, device.Location, device.LastSeen);
    }
}
