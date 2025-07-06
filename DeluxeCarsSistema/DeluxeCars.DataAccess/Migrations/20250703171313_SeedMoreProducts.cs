using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DeluxeCars.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class SeedMoreProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Productos",
                columns: new[] { "Id", "Descripcion", "Estado", "IdCategoria", "ImagenUrl", "Nombre", "OriginalEquipamentManufacture", "Precio", "StockMaximo", "StockMinimo", "UltimoPrecioCompra", "UnidadMedida" },
                values: new object[,]
                {
                    { 11, "Filtro de gasolina para Chevrolet Sail, Onix.", true, 4, null, "Filtro de Combustible", "FC-5501", 65000m, null, 20, 38000m, "Unidad" },
                    { 12, "Terminal axial para Kia Rio, Hyundai Accent.", true, 2, null, "Terminal de Dirección", "TR-3320", 95000m, null, 10, 55000m, "Unidad" },
                    { 13, "Alternador genérico de alta capacidad para varios modelos.", true, 5, null, "Alternador 12V 90A", "ALT-9001", 850000m, null, 3, 600000m, "Unidad" },
                    { 14, "Bomba de agua con empaque para Ford Fiesta y Ecosport.", true, 3, null, "Bomba de Agua", "BMA-205B", 280000m, null, 5, 180000m, "Unidad" },
                    { 15, "Botella de 500ml de líquido de frenos sintético.", true, 1, null, "Líquido de Frenos DOT 4", "LF-DOT4-500", 38000m, null, 30, 22000m, "Botella" },
                    { 16, "Aceite sintético para caja de cambios manual.", true, 6, null, "Aceite para Transmisión 75W-90", "OIL-75W90", 90000m, null, 12, 65000m, "Litro" },
                    { 17, "Filtro de aire para cabina con carbón activado anti-olores.", true, 4, null, "Filtro de Cabina Carbón Activado", "FC-C220", 55000m, null, 15, 30000m, "Unidad" },
                    { 18, "Bieleta o 'lápiz' de barra estabilizadora para Mazda 2.", true, 2, null, "Bieleta de Suspensión Delantera", "BIE-1090", 60000m, null, 20, 35000m, "Unidad" },
                    { 19, "Empaque de culata multilámina para Renault Logan/Sandero 1.6L 8V.", true, 3, null, "Empaque de Culata", "EMP-C16", 120000m, null, 5, 75000m, "Unidad" },
                    { 20, "Sensor de oxígeno de 4 pines, conector universal.", true, 5, null, "Sensor de Oxígeno (Sonda Lambda)", "O2S-4P", 250000m, null, 8, 160000m, "Unidad" },
                    { 21, "Bomba de freno principal para Nissan March / Versa.", true, 1, null, "Cilindro Maestro de Freno", "CMF-88A", 310000m, null, 4, 220000m, "Unidad" },
                    { 22, "Anillos sincronizadores para caja de Renault Twingo.", true, 6, null, "Sincronizador de 3ra y 4ta", "SYNC-34-RN", 380000m, null, 3, 250000m, "Juego" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 22);
        }
    }
}
