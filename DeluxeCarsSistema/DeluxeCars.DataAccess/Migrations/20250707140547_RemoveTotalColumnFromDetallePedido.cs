using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeluxeCars.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTotalColumnFromDetallePedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Total",
                table: "DetallesPedidos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Total",
                table: "DetallesPedidos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
