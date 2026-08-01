using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartSpaces.Application.Common.Interfaces;
using SmartSpaces.Application.Features.Users.Queries.GetUsers;

namespace SmartSpaces.Application.Features.Users.Queries.GetUserById;

public record GetUserByIdQuery(Guid Id) : IRequest<UserListItemDto>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserListItemDto>
{
    private readonly IApplicationDbContext _context;

    public GetUserByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserListItemDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException("Usuario no encontrado.");
        }

        return new UserListItemDto(user.Id, user.Name, user.Email, user.Folio, user.Role, user.Status);
    }
}
