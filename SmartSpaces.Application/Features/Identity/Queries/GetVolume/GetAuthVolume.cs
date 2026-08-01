using System.Globalization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartSpaces.Application.Common.Interfaces;

namespace SmartSpaces.Application.Features.Identity.Queries.GetVolume;

/// <summary>Periodos que manda el selector del panel: today | 7d | 30d | 90d.</summary>
public record GetAuthVolumeQuery(string? Period = null) : IRequest<IReadOnlyList<AuthVolumePointDto>>;

public record AuthVolumePointDto(string Time, int Value);

public class GetAuthVolumeQueryHandler : IRequestHandler<GetAuthVolumeQuery, IReadOnlyList<AuthVolumePointDto>>
{
    private static readonly CultureInfo Spanish = new("es-ES");

    private readonly IApplicationDbContext _context;

    public GetAuthVolumeQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AuthVolumePointDto>> Handle(GetAuthVolumeQuery request, CancellationToken cancellationToken)
    {
        var period = (request.Period ?? "today").Trim().ToLowerInvariant();
        var now = DateTime.UtcNow;

        // (inicio del primer bucket, tamaño del bucket, número de buckets) por periodo.
        var (windowStart, bucketSize, bucketCount) = period switch
        {
            "7d" => (now.Date.AddDays(-6), TimeSpan.FromDays(1), 7),
            "30d" => (now.Date.AddDays(-29), TimeSpan.FromDays(2), 15),
            "90d" => (now.Date.AddDays(-89), TimeSpan.FromDays(6), 15),
            _ => (now.Date, TimeSpan.FromHours(2), 12) // "today": 12 tramos de 2 h
        };

        var windowEnd = windowStart.Add(bucketSize * bucketCount);

        var timestamps = await _context.Sessions
            .AsNoTracking()
            .Where(s => s.CreatedAt >= windowStart && s.CreatedAt < windowEnd)
            .Select(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

        var buckets = new int[bucketCount];

        foreach (var timestamp in timestamps)
        {
            var index = (int)((timestamp - windowStart).Ticks / bucketSize.Ticks);

            if (index >= 0 && index < bucketCount)
            {
                buckets[index]++;
            }
        }

        return Enumerable.Range(0, bucketCount)
            .Select(i => new AuthVolumePointDto(
                FormatLabel(period, windowStart.Add(bucketSize * i)),
                buckets[i]))
            .ToList();
    }

    private static string FormatLabel(string period, DateTime bucketStart) => period switch
    {
        "7d" => bucketStart.ToString("ddd", Spanish),          // lun, mar, mié…
        "30d" or "90d" => bucketStart.ToString("d MMM", Spanish), // 5 jul
        _ => bucketStart.ToString("HH:00", CultureInfo.InvariantCulture)
    };
}
