using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartSpaces.Application.Common.Interfaces;

namespace SmartSpaces.Application.Features.Knowledge.Queries.GetQueries;

public record GetKnowledgeQueriesQuery(int Limit = 50) : IRequest<IReadOnlyList<KnowledgeQueryDto>>;

public record KnowledgeQueryDto(
    Guid Id,
    Guid UserId,
    string? UserName,
    string Source,
    string Question,
    string Answer,
    bool IsMock,
    double Confidence,
    DateTime Timestamp
);

public class GetKnowledgeQueriesQueryHandler : IRequestHandler<GetKnowledgeQueriesQuery, IReadOnlyList<KnowledgeQueryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetKnowledgeQueriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<KnowledgeQueryDto>> Handle(GetKnowledgeQueriesQuery request, CancellationToken cancellationToken)
    {
        var limit = request.Limit is > 0 and <= 200 ? request.Limit : 50;

        return await _context.KnowledgeQueries
            .AsNoTracking()
            .OrderByDescending(q => q.CreatedAt)
            .Take(limit)
            .Select(q => new KnowledgeQueryDto(
                q.Id,
                q.UserId,
                q.User != null ? q.User.Name : null,
                q.Source,
                q.Question,
                q.Answer,
                q.IsMock,
                q.Confidence,
                q.CreatedAt
            ))
            .ToListAsync(cancellationToken);
    }
}
