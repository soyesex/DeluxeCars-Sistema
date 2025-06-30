using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeluxeCarsDesktop.Migrations
{
    /// <inheritdoc />
    public partial class AddReceptionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CantidadRecibida",
                table: "DetallesPedidos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NotaRecepcion",
                table: "DetallesPedidos",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CantidadRecibida",
                table: "DetallesPedidos");

            migrationBuilder.DropColumn(
                name: "NotaRecepcion",
                table: "DetallesPedidos");
        }
    }
}
