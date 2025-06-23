using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DeluxeCarsDesktop.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureModelAndSeedPaymentMethods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AplicaParaCompras",
                table: "MetodosPago",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AplicaParaVentas",
                table: "MetodosPago",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "ComisionPorcentaje",
                table: "MetodosPago",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiereReferencia",
                table: "MetodosPago",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Tipo",
                table: "MetodosPago",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "MetodosPago",
                columns: new[] { "Id", "AplicaParaCompras", "AplicaParaVentas", "Codigo", "ComisionPorcentaje", "Descripcion", "Disponible", "RequiereReferencia", "Tipo" },
                values: new object[,]
                {
                    { 1, true, true, "EFE", null, "Efectivo", true, false, 0 },
                    { 2, false, true, "TDC", null, "Tarjeta de Crédito", true, false, 2 }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, "Acceso total al sistema.", "Administrador" },
                    { 2, "Acceso limitado a ventas y operaciones diarias.", "Empleado" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MetodosPago",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "MetodosPago",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DropColumn(
                name: "AplicaParaCompras",
                table: "MetodosPago");

            migrationBuilder.DropColumn(
                name: "AplicaParaVentas",
                table: "MetodosPago");

            migrationBuilder.DropColumn(
                name: "ComisionPorcentaje",
                table: "MetodosPago");

            migrationBuilder.DropColumn(
                name: "RequiereReferencia",
                table: "MetodosPago");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "MetodosPago");
        }
    }
}
