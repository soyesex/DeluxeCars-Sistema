using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeluxeCars.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailConfigurationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailEmisor",
                table: "Configuraciones",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnableSsl",
                table: "Configuraciones",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte[]>(
                name: "PasswordEmailEmisor",
                table: "Configuraciones",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpHost",
                table: "Configuraciones",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SmtpPort",
                table: "Configuraciones",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Configuraciones",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "EmailEmisor", "EnableSsl", "PasswordEmailEmisor", "SmtpHost", "SmtpPort" },
                values: new object[] { "", true, null, "", 587 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailEmisor",
                table: "Configuraciones");

            migrationBuilder.DropColumn(
                name: "EnableSsl",
                table: "Configuraciones");

            migrationBuilder.DropColumn(
                name: "PasswordEmailEmisor",
                table: "Configuraciones");

            migrationBuilder.DropColumn(
                name: "SmtpHost",
                table: "Configuraciones");

            migrationBuilder.DropColumn(
                name: "SmtpPort",
                table: "Configuraciones");
        }
    }
}
