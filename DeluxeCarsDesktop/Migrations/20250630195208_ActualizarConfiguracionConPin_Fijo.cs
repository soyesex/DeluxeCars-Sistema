using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeluxeCarsDesktop.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarConfiguracionConPin_Fijo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Configuraciones",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AdminPINHash", "AdminPINSalt" },
                values: new object[] { new byte[] { 46, 201, 139, 121, 254, 155, 209, 14, 115, 229, 91, 103, 255, 176, 30, 131, 122, 65, 235, 77, 6, 150, 70, 115, 63, 221, 60, 247, 33, 228, 2, 101, 195, 83, 0, 52, 72, 89, 198, 33, 224, 70, 233, 125, 177, 74, 109, 102, 23, 223, 61, 33, 32, 20, 58, 102, 179, 205, 31, 83, 178, 63, 15, 90 }, new byte[] { 200, 83, 160, 16, 78, 248, 207, 38, 92, 77, 89, 102, 153, 114, 81, 115, 83, 30, 186, 233, 102, 213, 73, 2, 96, 217, 113, 195, 170, 196, 242, 251, 129, 207, 218, 130, 213, 87, 159, 92, 195, 57, 218, 61, 141, 228, 57, 69, 141, 252, 79, 232, 234, 248, 158, 153, 105, 6, 107, 229, 59, 2, 96, 151, 68, 31, 241, 195, 1, 90, 56, 145, 138, 20, 210, 114, 191, 199, 75, 61, 21, 160, 101, 184, 223, 64, 29, 126, 194, 161, 64, 192, 154, 154, 89, 83, 176, 121, 121, 121, 206, 132, 229, 52, 47, 239, 88, 93, 102, 226, 96, 15, 61, 150, 137, 82, 38, 54, 163, 54, 136, 220, 154, 111, 209, 217, 250, 29 } });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Configuraciones",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AdminPINHash", "AdminPINSalt" },
                values: new object[] { null, null });
        }
    }
}
