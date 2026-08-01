using SmartSpaces.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace SmartSpaces.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Session> Sessions { get; }
    DbSet<KnowledgeQuery> KnowledgeQueries { get; }
    DbSet<AccessLog> AccessLogs { get; }
    DbSet<Device> Devices { get; }
    DbSet<AccessPoint> AccessPoints { get; }
    DbSet<KnowledgeDocument> KnowledgeDocuments { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}