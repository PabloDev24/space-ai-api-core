using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartSpaces.Application.Common.Interfaces;

namespace SmartSpaces.Application.Features.Users.Queries.GetUsers;

public record GetUsersQuery(string? Role = null, string? Search = null) : IRequest<IReadOnlyList<UserListItemDto>>;

public record UserListItemDto(Guid Id, string Name, string Email, string? Folio, string Role);

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, IReadOnlyList<UserListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUsersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<UserListItemDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            var role = request.Role.ToLower();
            query = query.Where(u => u.Role.ToLower() == role);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(u =>
                u.Name.ToLower().Contains(search) ||
                u.Email.ToLower().Contains(search) ||
                (u.Folio != null && u.Folio.ToLower().Contains(search)));
        }

        return await query
            .OrderBy(u => u.Name)
            .Select(u => new UserListItemDto(u.Id, u.Name, u.Email, u.Folio, u.Role))
            .ToListAsync(cancellationToken);
    }
}
