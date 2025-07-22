using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeluxeCars.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddFacturaTotalColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SubTotal",
                table: "Facturas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Total",
                table: "Facturas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalIVA",
                table: "Facturas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "Total",
                table: "DetallesPedidos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldComputedColumnSql: "((Cantidad * PrecioUnitario - ISNULL(Descuento, 0)) * (1 + ISNULL(IVA, 0)/100))");

            migrationBuilder.AlterColumn<decimal>(
                name: "Total",
                table: "DetallesFactura",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldComputedColumnSql: "((Cantidad * PrecioUnitario - ISNULL(Descuento, 0)) * (1 + ISNULL(IVA, 0)/100))");

            migrationBuilder.AlterColumn<decimal>(
                name: "SubTotalLinea",
                table: "DetallesFactura",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldComputedColumnSql: "(Cantidad * PrecioUnitario - ISNULL(Descuento, 0))");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubTotal",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "Total",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "TotalIVA",
                table: "Facturas");

            migrationBuilder.AlterColumn<decimal>(
                name: "Total",
                table: "DetallesPedidos",
                type: "decimal(18,2)",
                nullable: false,
                computedColumnSql: "((Cantidad * PrecioUnitario - ISNULL(Descuento, 0)) * (1 + ISNULL(IVA, 0)/100))",
                stored: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Total",
                table: "DetallesFactura",
                type: "decimal(18,2)",
                nullable: false,
                computedColumnSql: "((Cantidad * PrecioUnitario - ISNULL(Descuento, 0)) * (1 + ISNULL(IVA, 0)/100))",
                stored: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "SubTotalLinea",
                table: "DetallesFactura",
                type: "decimal(18,2)",
                nullable: false,
                computedColumnSql: "(Cantidad * PrecioUnitario - ISNULL(Descuento, 0))",
                stored: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }
    }
}
