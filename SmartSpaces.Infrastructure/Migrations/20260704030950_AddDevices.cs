using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartSpaces.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDevices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Location = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    LastSeen = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Devices",
                columns: new[] { "Id", "Code", "LastSeen", "Location", "Name", "Status", "Type" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111101"), "cart-tablet-001", new DateTime(2026, 7, 1, 18, 0, 0, 0, DateTimeKind.Utc), "Pasillo Principal", "Carrito Inteligente Asistido", "ONLINE", "CART" },
                    { new Guid("11111111-1111-1111-1111-111111111102"), "access-tablet-001", new DateTime(2026, 7, 1, 18, 0, 0, 0, DateTimeKind.Utc), "Entrada Principal", "Acceso Principal", "ONLINE", "ACCESS" },
                    { new Guid("11111111-1111-1111-1111-111111111103"), "side-tablet-001", new DateTime(2026, 7, 1, 18, 0, 0, 0, DateTimeKind.Utc), "Recepción", "SIDE Tablet Principal", "OFFLINE", "SIDE" },
                    { new Guid("11111111-1111-1111-1111-111111111104"), "sensor-001", new DateTime(2026, 7, 1, 18, 0, 0, 0, DateTimeKind.Utc), "Edificio B", "Sensor de Ocupación B-204", "ONLINE", "SENSOR" },
                    { new Guid("11111111-1111-1111-1111-111111111105"), "camera-001", new DateTime(2026, 7, 1, 18, 0, 0, 0, DateTimeKind.Utc), "Entrada Principal", "Cámara Entrada Principal", "ONLINE", "CAMERA" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "Folio", "Name", "PasswordHash", "QrExpiry", "QrToken", "Role" },
                values: new object[] { new Guid("22222222-2222-2222-2222-222222222201"), "daniel@utl.edu.mx", "20260001", "Daniel Ojeda Luna", "$2a$11$hIrQTKYZvJcz/HbzJVI6O.glhMiSwEqstSC2emQIUchXjltox.fci", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "admin" });

            migrationBuilder.CreateIndex(
                name: "IX_Devices_Code",
                table: "Devices",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222201"));
        }
    }
}
