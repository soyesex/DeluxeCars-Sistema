using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeluxeCarsDesktop.Migrations
{
    /// <inheritdoc />
    public partial class AddConfiguracionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Configuraciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreTienda = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HorarioAtencion = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PorcentajeIVA = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    AdminPINHash = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    AdminPINSalt = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Logo = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Banner = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configuraciones", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Configuraciones",
                columns: new[] { "Id", "AdminPINHash", "AdminPINSalt", "Banner", "Direccion", "Email", "HorarioAtencion", "Logo", "NombreTienda", "PorcentajeIVA", "Telefono" },
                values: new object[] { 1, null, null, null, "La rosita", "deluxecars@gmail.com", "Lunes a Viernes de 8am a 6pm", null, "Deluxe Cars", 19.0m, "3001234567" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Configuraciones");
        }
    }
}
