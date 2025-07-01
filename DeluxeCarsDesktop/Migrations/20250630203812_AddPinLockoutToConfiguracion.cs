using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeluxeCarsDesktop.Migrations
{
    /// <inheritdoc />
    public partial class AddPinLockoutToConfiguracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LockoutEndTimeUtc",
                table: "Configuraciones",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PinFailedAttempts",
                table: "Configuraciones",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Configuraciones",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "LockoutEndTimeUtc", "PinFailedAttempts" },
                values: new object[] { null, 0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LockoutEndTimeUtc",
                table: "Configuraciones");

            migrationBuilder.DropColumn(
                name: "PinFailedAttempts",
                table: "Configuraciones");
        }
    }
}
