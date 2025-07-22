using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeluxeCars.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificacionesActivasSwitch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NotificacionesActivas",
                table: "Configuraciones",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Configuraciones",
                keyColumn: "Id",
                keyValue: 1,
                column: "NotificacionesActivas",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotificacionesActivas",
                table: "Configuraciones");
        }
    }
}
