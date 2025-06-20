using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeluxeCarsDesktop.Migrations
{
    /// <inheritdoc />
    public partial class AddStockMinMaxAndRemoveOldFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Borrar la clave foránea usando el NOMBRE REAL de tu base de datos
            migrationBuilder.DropForeignKey(
                name: "FK__DetallesP__IdPro__5BE2A6F2", // <-- EL NOMBRE REAL QUE ENCONTRASTE
                table: "DetallesPedidos");

            // 2. Borrar el índice usando el nombre que EF debió crear para la columna IdProducto.
            //    El nombre del índice casi siempre sigue el formato IX_Tabla_Columna.
            //migrationBuilder.DropIndex(
            //    name: "IX_DetallesPedidos_IdProducto", // <-- CAMBIAMOS "ProductoId" por "IdProducto"
            //    table: "DetallesPedidos");

            migrationBuilder.DropColumn(
                name: "Lote",
                table: "Productos");

            migrationBuilder.AlterColumn<byte[]>(
                name: "PasswordSalt",
                table: "Usuarios",
                type: "varbinary(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(16)",
                oldMaxLength: 16);

            migrationBuilder.AddColumn<int>(
                name: "StockMaximo",
                table: "Productos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StockMinimo",
                table: "Productos",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Token",
                table: "PasswordResets",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalIVA",
                table: "Facturas",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Total",
                table: "Facturas",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "SubTotal",
                table: "Facturas",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Observaciones",
                table: "Facturas",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesPedidos_IdProducto",
                table: "DetallesPedidos",
                column: "IdProducto");

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesPedidos_Productos_IdProducto",
                table: "DetallesPedidos",
                column: "IdProducto",
                principalTable: "Productos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetallesPedidos_Productos_IdProducto",
                table: "DetallesPedidos");

            migrationBuilder.DropIndex(
                name: "IX_DetallesPedidos_IdProducto",
                table: "DetallesPedidos");

            migrationBuilder.DropColumn(
                name: "ProfilePicture",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "StockMaximo",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "StockMinimo",
                table: "Productos");

            migrationBuilder.AlterColumn<byte[]>(
                name: "PasswordSalt",
                table: "Usuarios",
                type: "varbinary(16)",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(128)",
                oldMaxLength: 128);

            migrationBuilder.AddColumn<string>(
                name: "Lote",
                table: "Productos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Token",
                table: "PasswordResets",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldDefaultValueSql: "NEWID()");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalIVA",
                table: "Facturas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Total",
                table: "Facturas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SubTotal",
                table: "Facturas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Observaciones",
                table: "Facturas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductoId",
                table: "DetallesPedidos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_DetallesPedidos_ProductoId",
                table: "DetallesPedidos",
                column: "ProductoId");

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesPedidos_Productos_ProductoId",
                table: "DetallesPedidos",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
