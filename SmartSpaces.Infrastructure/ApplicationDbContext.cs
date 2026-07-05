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
    public DbSet<AccessLog> AccessLogs => Set<AccessLog>();
    public DbSet<Device> Devices => Set<Device>();
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

        modelBuilder.Entity<AccessLog>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.DeviceId).HasMaxLength(100);
            entity.Property(e => e.Direction).HasMaxLength(10);

            entity.HasOne(a => a.User)
                    .WithMany()
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();

            entity.Property(e => e.Code).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Type).HasMaxLength(30);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.Location).HasMaxLength(150);
        });

        // Datos demo para MVP (docs/00 §6 P0 "hay datos demo") — ver docs/03_API_MINIMUM_CONTRACTS.txt §7, §12.2.
        var seedTimestamp = new DateTime(2026, 7, 1, 18, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Device>().HasData(
            new Device
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111101"),
                Code = "cart-tablet-001",
                Name = "Carrito Inteligente Asistido",
                Type = "CART",
                Status = "ONLINE",
                Location = "Pasillo Principal",
                LastSeen = seedTimestamp
            },
            new Device
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111102"),
                Code = "access-tablet-001",
                Name = "Acceso Principal",
                Type = "ACCESS",
                Status = "ONLINE",
                Location = "Entrada Principal",
                LastSeen = seedTimestamp
            },
            new Device
            {
                // Registro de metadata únicamente: la vista SIDE queda fuera de alcance en esta ronda.
                Id = Guid.Parse("11111111-1111-1111-1111-111111111103"),
                Code = "side-tablet-001",
                Name = "SIDE Tablet Principal",
                Type = "SIDE",
                Status = "OFFLINE",
                Location = "Recepción",
                LastSeen = seedTimestamp
            },
            new Device
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111104"),
                Code = "sensor-001",
                Name = "Sensor de Ocupación B-204",
                Type = "SENSOR",
                Status = "ONLINE",
                Location = "Edificio B",
                LastSeen = seedTimestamp
            },
            new Device
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111105"),
                Code = "camera-001",
                Name = "Cámara Entrada Principal",
                Type = "CAMERA",
                Status = "ONLINE",
                Location = "Entrada Principal",
                LastSeen = seedTimestamp
            }
        );

        // Usuario admin demo para poder iniciar sesión en el panel una vez activados los guards de rol.
        // Password demo: "Admin123!" (hash BCrypt.Net-Next 4.2.0 pre-calculado — no generar en runtime, rompería la migration).
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222201"),
                Name = "Daniel Ojeda Luna",
                Email = "daniel@utl.edu.mx",
                PasswordHash = "$2a$11$hIrQTKYZvJcz/HbzJVI6O.glhMiSwEqstSC2emQIUchXjltox.fci",
                Folio = "20260001",
                Role = "admin",
                QrToken = null,
                QrExpiry = default
            }
        );
    }
}
