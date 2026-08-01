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

    //Nuevo
    public DbSet<Materia> Materias => Set<Materia>();
    public DbSet<Calificacion> Calificaciones => Set<Calificacion>();
    public DbSet<ClaseHorario> ClasesHorario => Set<ClaseHorario>();
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

        //Nuevo
        modelBuilder.Entity<Materia>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(150);
            entity.Property(e => e.Profesor).HasMaxLength(150);
        });

        modelBuilder.Entity<Calificacion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(c => c.Materia).WithMany(m => m.Calificaciones).HasForeignKey(c => c.MateriaId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ClaseHorario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Edificio).HasMaxLength(100);
            entity.Property(e => e.Salon).HasMaxLength(50);
            entity.HasOne(c => c.Materia).WithMany(m => m.Clases).HasForeignKey(c => c.MateriaId).OnDelete(DeleteBehavior.Cascade);
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
        // Password demo: "SpaceIA2026!" (hash BCrypt.Net-Next 4.2.0 pre-calculado — no generar en runtime, rompería la migration).
        // Rotado 2026-07-12: el valor anterior ("Admin123!") quedó expuesto en el historial de git de este repo.
        //Nuevo
        // Actualiza el seed de User existente agregando los campos académicos:
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222201"),
                Name = "Daniel Ojeda Luna",
                Email = "daniel@utl.edu.mx",
                PasswordHash = "$2a$11$tHMJ.UMKAT.LxfTpqbtRz.Trd4yOSVBl1ugCtQMeEK1dK8gVIq4KK",
                Folio = "20260001",
                Role = "admin",
                QrToken = null,
                QrExpiry = default,
                Matricula = "20260001",
                Carrera = "Ingeniería en Desarrollo y Gestión de Software",
                Grupo = "IDGS-7A",
                Division = "División de Tecnologías de la Información",
                Campus = "UTL Campus León",
                Telefono = "4771234567",
                TotalAttendance = 92
            },
            new User
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222202"),
                Name = "Alumno de Prueba",
                Email = "alumno@utl.edu.mx",
                PasswordHash = "$2a$11$tHMJ.UMKAT.LxfTpqbtRz.Trd4yOSVBl1ugCtQMeEK1dK8gVIq4KK", // mismo hash de prueba
                Folio = "20260002",
                Role = "student",
                QrToken = null,
                QrExpiry = default,
                Matricula = "20260002",
                Carrera = "Ingeniería en Desarrollo y Gestión de Software",
                Grupo = "IDGS-7A",
                Division = "División de Tecnologías de la Información",
                Campus = "UTL Campus León",
                Telefono = "4779876543",
                TotalAttendance = 88
            }
        );

        // --- Materias ---
        modelBuilder.Entity<Materia>().HasData(
            new Materia { Id = Guid.Parse("33333333-3333-3333-3333-333333333301"), Nombre = "Programación Móvil", Profesor = "Ing. Laura Reyes" },
            new Materia { Id = Guid.Parse("33333333-3333-3333-3333-333333333302"), Nombre = "Bases de Datos Avanzadas", Profesor = "Ing. Marco Villalobos" },
            new Materia { Id = Guid.Parse("33333333-3333-3333-3333-333333333303"), Nombre = "Ingeniería de Software", Profesor = "Ing. Paola Sánchez" }
        );

        // --- Calificaciones (para ambos usuarios de prueba) ---
        modelBuilder.Entity<Calificacion>().HasData(
            new Calificacion { Id = Guid.Parse("44444444-4444-4444-4444-444444444401"), UserId = Guid.Parse("22222222-2222-2222-2222-222222222202"), MateriaId = Guid.Parse("33333333-3333-3333-3333-333333333301"), Parcial1 = 9.0, Parcial2 = 8.5, Parcial3 = 9.5, Final = 9.0 },
            new Calificacion { Id = Guid.Parse("44444444-4444-4444-4444-444444444402"), UserId = Guid.Parse("22222222-2222-2222-2222-222222222202"), MateriaId = Guid.Parse("33333333-3333-3333-3333-333333333302"), Parcial1 = 7.5, Parcial2 = 8.0, Parcial3 = null, Final = null },
            new Calificacion { Id = Guid.Parse("44444444-4444-4444-4444-444444444403"), UserId = Guid.Parse("22222222-2222-2222-2222-222222222202"), MateriaId = Guid.Parse("33333333-3333-3333-3333-333333333303"), Parcial1 = 10.0, Parcial2 = 9.5, Parcial3 = 9.0, Final = 9.5 }
        );

        // --- ClasesHorario (por grupo IDGS-7A) ---
        modelBuilder.Entity<ClaseHorario>().HasData(
            new ClaseHorario { Id = Guid.Parse("55555555-5555-5555-5555-555555555501"), MateriaId = Guid.Parse("33333333-3333-3333-3333-333333333301"), Grupo = "IDGS-7A", HoraInicio = new TimeSpan(8, 0, 0), HoraFin = new TimeSpan(9, 30, 0), Edificio = "Edificio A", Salon = "A-204", DiaSemana = DayOfWeek.Monday },
            new ClaseHorario { Id = Guid.Parse("55555555-5555-5555-5555-555555555502"), MateriaId = Guid.Parse("33333333-3333-3333-3333-333333333302"), Grupo = "IDGS-7A", HoraInicio = new TimeSpan(9, 30, 0), HoraFin = new TimeSpan(11, 0, 0), Edificio = "Edificio B", Salon = "B-101", DiaSemana = DayOfWeek.Monday },
            new ClaseHorario { Id = Guid.Parse("55555555-5555-5555-5555-555555555503"), MateriaId = Guid.Parse("33333333-3333-3333-3333-333333333303"), Grupo = "IDGS-7A", HoraInicio = new TimeSpan(8, 0, 0), HoraFin = new TimeSpan(9, 30, 0), Edificio = "Edificio A", Salon = "A-101", DiaSemana = DayOfWeek.Tuesday }
        );
        //
    }
}
