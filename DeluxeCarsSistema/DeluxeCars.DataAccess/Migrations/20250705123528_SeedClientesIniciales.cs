using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeluxeCars.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class SeedClientesIniciales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "ComisionPorcentaje",
                table: "MetodosPago",
                type: "decimal(5,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Direccion",
                table: "Clientes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "Clientes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "IdCiudad",
                table: "Clientes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Identificacion",
                table: "Clientes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TipoCliente",
                table: "Clientes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TipoIdentificacion",
                table: "Clientes",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Direccion", "FechaCreacion", "IdCiudad", "Identificacion", "TipoCliente", "TipoIdentificacion" },
                values: new object[] { "Calle Falsa 123", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "900123456-7", "Taller", "NIT" });

            migrationBuilder.UpdateData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Direccion", "FechaCreacion", "IdCiudad", "Identificacion", "TipoCliente", "TipoIdentificacion" },
                values: new object[] { "Avenida Siempre Viva 45", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "1098765432", "Persona Natural", "CC" });

            migrationBuilder.InsertData(
                table: "Clientes",
                columns: new[] { "Id", "Direccion", "Email", "Estado", "FechaCreacion", "IdCiudad", "Identificacion", "Nombre", "Telefono", "TipoCliente", "TipoIdentificacion" },
                values: new object[] { 1, "N/A", "consumidor@final.com", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "999999999", "Consumidor Final", "N/A", "Persona Natural", "N/A" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DropColumn(
                name: "Direccion",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "IdCiudad",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Identificacion",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "TipoCliente",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "TipoIdentificacion",
                table: "Clientes");

            migrationBuilder.AlterColumn<decimal>(
                name: "ComisionPorcentaje",
                table: "MetodosPago",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldNullable: true);
        }
    }
}
