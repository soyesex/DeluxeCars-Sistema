using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeluxeCarsDesktop.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierPaymentModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UnidadMedida",
                table: "Productos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<decimal>(
                name: "CostoUnitario",
                table: "MovimientosInventario",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "PagosProveedores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProveedor = table.Column<int>(type: "int", nullable: false),
                    IdMetodoPago = table.Column<int>(type: "int", nullable: false),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    MontoPagado = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Referencia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notas = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagosProveedores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PagosProveedores_MetodosPago_IdMetodoPago",
                        column: x => x.IdMetodoPago,
                        principalTable: "MetodosPago",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PagosProveedores_Proveedores_IdProveedor",
                        column: x => x.IdProveedor,
                        principalTable: "Proveedores",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PagosProveedores_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PagoProveedorPedidos",
                columns: table => new
                {
                    IdPagoProveedor = table.Column<int>(type: "int", nullable: false),
                    IdPedido = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagoProveedorPedidos", x => new { x.IdPagoProveedor, x.IdPedido });
                    table.ForeignKey(
                        name: "FK_PagoProveedorPedidos_PagosProveedores_IdPagoProveedor",
                        column: x => x.IdPagoProveedor,
                        principalTable: "PagosProveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PagoProveedorPedidos_Pedidos_IdPedido",
                        column: x => x.IdPedido,
                        principalTable: "Pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PagoProveedorPedidos_IdPedido",
                table: "PagoProveedorPedidos",
                column: "IdPedido");

            migrationBuilder.CreateIndex(
                name: "IX_PagosProveedores_IdMetodoPago",
                table: "PagosProveedores",
                column: "IdMetodoPago");

            migrationBuilder.CreateIndex(
                name: "IX_PagosProveedores_IdProveedor",
                table: "PagosProveedores",
                column: "IdProveedor");

            migrationBuilder.CreateIndex(
                name: "IX_PagosProveedores_IdUsuario",
                table: "PagosProveedores",
                column: "IdUsuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PagoProveedorPedidos");

            migrationBuilder.DropTable(
                name: "PagosProveedores");

            migrationBuilder.AlterColumn<string>(
                name: "UnidadMedida",
                table: "Productos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CostoUnitario",
                table: "MovimientosInventario",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }
    }
}
