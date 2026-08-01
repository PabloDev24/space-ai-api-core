using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartSpaces.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Actualizacion_Modelos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Campus",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Carrera",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Division",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailAlterno",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Grupo",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Matricula",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalAttendance",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Materias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Profesor = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Calificaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MateriaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Parcial1 = table.Column<double>(type: "double precision", nullable: true),
                    Parcial2 = table.Column<double>(type: "double precision", nullable: true),
                    Parcial3 = table.Column<double>(type: "double precision", nullable: true),
                    Final = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Calificaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Calificaciones_Materias_MateriaId",
                        column: x => x.MateriaId,
                        principalTable: "Materias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Calificaciones_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClasesHorario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MateriaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Grupo = table.Column<string>(type: "text", nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "interval", nullable: false),
                    HoraFin = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Edificio = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Salon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DiaSemana = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClasesHorario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClasesHorario_Materias_MateriaId",
                        column: x => x.MateriaId,
                        principalTable: "Materias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Materias",
                columns: new[] { "Id", "Nombre", "Profesor" },
                values: new object[,]
                {
                    { new Guid("33333333-3333-3333-3333-333333333301"), "Programación Móvil", "Ing. Laura Reyes" },
                    { new Guid("33333333-3333-3333-3333-333333333302"), "Bases de Datos Avanzadas", "Ing. Marco Villalobos" },
                    { new Guid("33333333-3333-3333-3333-333333333303"), "Ingeniería de Software", "Ing. Paola Sánchez" }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222201"),
                columns: new[] { "Campus", "Carrera", "Division", "EmailAlterno", "Grupo", "Matricula", "Telefono", "TotalAttendance" },
                values: new object[] { "UTL Campus León", "Ingeniería en Desarrollo y Gestión de Software", "División de Tecnologías de la Información", null, "IDGS-7A", "20260001", "4771234567", 92 });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Campus", "Carrera", "Division", "Email", "EmailAlterno", "Folio", "Grupo", "Matricula", "Name", "PasswordHash", "QrExpiry", "QrToken", "Role", "Telefono", "TotalAttendance" },
                values: new object[] { new Guid("22222222-2222-2222-2222-222222222202"), "UTL Campus León", "Ingeniería en Desarrollo y Gestión de Software", "División de Tecnologías de la Información", "alumno@utl.edu.mx", null, "20260002", "IDGS-7A", "20260002", "Alumno de Prueba", "$2a$11$tHMJ.UMKAT.LxfTpqbtRz.Trd4yOSVBl1ugCtQMeEK1dK8gVIq4KK", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "student", "4779876543", 88 });

            migrationBuilder.InsertData(
                table: "Calificaciones",
                columns: new[] { "Id", "Final", "MateriaId", "Parcial1", "Parcial2", "Parcial3", "UserId" },
                values: new object[,]
                {
                    { new Guid("44444444-4444-4444-4444-444444444401"), 9.0, new Guid("33333333-3333-3333-3333-333333333301"), 9.0, 8.5, 9.5, new Guid("22222222-2222-2222-2222-222222222202") },
                    { new Guid("44444444-4444-4444-4444-444444444402"), null, new Guid("33333333-3333-3333-3333-333333333302"), 7.5, 8.0, null, new Guid("22222222-2222-2222-2222-222222222202") },
                    { new Guid("44444444-4444-4444-4444-444444444403"), 9.5, new Guid("33333333-3333-3333-3333-333333333303"), 10.0, 9.5, 9.0, new Guid("22222222-2222-2222-2222-222222222202") }
                });

            migrationBuilder.InsertData(
                table: "ClasesHorario",
                columns: new[] { "Id", "DiaSemana", "Edificio", "Grupo", "HoraFin", "HoraInicio", "MateriaId", "Salon" },
                values: new object[,]
                {
                    { new Guid("55555555-5555-5555-5555-555555555501"), 1, "Edificio A", "IDGS-7A", new TimeSpan(0, 9, 30, 0, 0), new TimeSpan(0, 8, 0, 0, 0), new Guid("33333333-3333-3333-3333-333333333301"), "A-204" },
                    { new Guid("55555555-5555-5555-5555-555555555502"), 1, "Edificio B", "IDGS-7A", new TimeSpan(0, 11, 0, 0, 0), new TimeSpan(0, 9, 30, 0, 0), new Guid("33333333-3333-3333-3333-333333333302"), "B-101" },
                    { new Guid("55555555-5555-5555-5555-555555555503"), 2, "Edificio A", "IDGS-7A", new TimeSpan(0, 9, 30, 0, 0), new TimeSpan(0, 8, 0, 0, 0), new Guid("33333333-3333-3333-3333-333333333303"), "A-101" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Calificaciones_MateriaId",
                table: "Calificaciones",
                column: "MateriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Calificaciones_UserId",
                table: "Calificaciones",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClasesHorario_MateriaId",
                table: "ClasesHorario",
                column: "MateriaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Calificaciones");

            migrationBuilder.DropTable(
                name: "ClasesHorario");

            migrationBuilder.DropTable(
                name: "Materias");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222202"));

            migrationBuilder.DropColumn(
                name: "Campus",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Carrera",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Division",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmailAlterno",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Grupo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Matricula",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Telefono",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TotalAttendance",
                table: "Users");
        }
    }
}
