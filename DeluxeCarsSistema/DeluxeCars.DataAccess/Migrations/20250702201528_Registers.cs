using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DeluxeCars.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Registers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Clientes",
                columns: new[] { "Id", "Email", "Estado", "Nombre", "Telefono" },
                values: new object[,]
                {
                    { 2, "compras@tallerrapido.com", true, "Taller \"El Rápido\"", "3009988776" },
                    { 3, "lucia.f@email.com", true, "Lucía Fernandez", "3215554433" }
                });

            migrationBuilder.InsertData(
                table: "Productos",
                columns: new[] { "Id", "Descripcion", "Estado", "IdCategoria", "ImagenUrl", "Nombre", "OriginalEquipamentManufacture", "Precio", "StockMaximo", "StockMinimo", "UltimoPrecioCompra", "UnidadMedida" },
                values: new object[,]
                {
                    { 6, "Disco de freno delantero para Chevrolet Captiva, Hyundai Tucson.", true, 1, null, "Disco de Freno Ventilado", "DK-455A", 220000m, null, 8, 150000m, "Unidad" },
                    { 7, "Rótula inferior para Toyota Hilux y Fortuner.", true, 2, null, "Rótula de Suspensión", "ROT-221B", 75000m, null, 15, 45000m, "Unidad" },
                    { 8, "Bujía de alto rendimiento NGK para una mejor combustión.", true, 3, null, "Bujía de Iridio", "IR-7700", 45000m, null, 20, 25000m, "Unidad" },
                    { 9, "Batería sellada MAC Gold Plus, libre de mantenimiento.", true, 5, null, "Batería 12V 850A", "BAT-12850", 550000m, null, 5, 390000m, "Unidad" },
                    { 10, "Kit de clutch completo LUK para Chevrolet Spark GT.", true, 6, null, "Kit de Embrague", "ACK-2105", 750000m, null, 4, 550000m, "Kit" }
                });

            migrationBuilder.InsertData(
                table: "Proveedores",
                columns: new[] { "Id", "Email", "Estado", "IdMunicipio", "NIT", "RazonSocial", "Telefono" },
                values: new object[,]
                {
                    { 1, "compras@elfrenazo.com", true, 7, "900.123.456-7", "AutoPartes El Frenazo S.A.S.", "3101234567" },
                    { 2, "contacto@ruedalibre.co", true, 12, "830.987.654-1", "Importaciones Rueda Libre Ltda.", "3159876543" },
                    { 3, "proveedor@pistonveloz.com", true, 16, "789.456.123-2", "Distribuciones El Pistón Veloz", "3005551212" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Proveedores",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Proveedores",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Proveedores",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
