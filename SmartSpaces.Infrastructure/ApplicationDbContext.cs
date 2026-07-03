using Microsoft.EntityFrameworkCore;
using SmartSpaces.Domain.Entities;
using SmartSpaces.Application.Common.Interfaces;

namespace SmartSpaces.Infrastructure.Persistence;
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<KnowledgeQuery> KnowledgeQueries => Set<KnowledgeQuery>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();

            entity.HasIndex(e => e.Folio).IsUnique();

            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.PasswordHash).IsRequired();
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(s => s.User)
                    .WithMany(u => u.Sessions)
                    .HasForeignKey(s => s.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<KnowledgeQuery>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Source).HasMaxLength(50);
            entity.Property(e => e.Question).HasMaxLength(1000);

            entity.HasOne(q => q.User)
                    .WithMany()
                    .HasForeignKey(q => q.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
