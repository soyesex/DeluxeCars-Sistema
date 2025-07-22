using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DeluxeCars.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class SeedCategorias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categorias",
                columns: new[] { "Id", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, "Componentes del sistema de frenado.", "Frenos" },
                    { 2, "Amortiguadores, rótulas y componentes de dirección.", "Suspensión y Dirección" },
                    { 3, "Partes internas y externas del motor.", "Motor" },
                    { 4, "Filtros de aceite, aire, combustible y cabina.", "Filtros" },
                    { 5, "Baterías, alternadores y sensores.", "Sistema Eléctrico" },
                    { 6, "Componentes de la caja de cambios y embrague.", "Transmisión" },
                    { 7, "Neumáticos y rines de varias medidas.", "Llantas y Rines" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 7);
        }
    }
}
