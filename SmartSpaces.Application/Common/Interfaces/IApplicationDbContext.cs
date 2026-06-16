using SmartSpaces.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace SmartSpaces.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Session> Sessions { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}