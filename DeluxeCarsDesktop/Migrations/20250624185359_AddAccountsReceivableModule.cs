using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeluxeCarsDesktop.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountsReceivableModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SaldoPendiente",
                table: "Facturas");

            migrationBuilder.CreateTable(
                name: "PagosClientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCliente = table.Column<int>(type: "int", nullable: false),
                    IdMetodoPago = table.Column<int>(type: "int", nullable: false),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    MontoRecibido = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Referencia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notas = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagosClientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PagosClientes_Clientes_IdCliente",
                        column: x => x.IdCliente,
                        principalTable: "Clientes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PagosClientes_MetodosPago_IdMetodoPago",
                        column: x => x.IdMetodoPago,
                        principalTable: "MetodosPago",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PagosClientes_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PagoClienteFacturas",
                columns: table => new
                {
                    IdPagoCliente = table.Column<int>(type: "int", nullable: false),
                    IdFactura = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagoClienteFacturas", x => new { x.IdPagoCliente, x.IdFactura });
                    table.ForeignKey(
                        name: "FK_PagoClienteFacturas_Facturas_IdFactura",
                        column: x => x.IdFactura,
                        principalTable: "Facturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PagoClienteFacturas_PagosClientes_IdPagoCliente",
                        column: x => x.IdPagoCliente,
                        principalTable: "PagosClientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PagoClienteFacturas_IdFactura",
                table: "PagoClienteFacturas",
                column: "IdFactura");

            migrationBuilder.CreateIndex(
                name: "IX_PagosClientes_IdCliente",
                table: "PagosClientes",
                column: "IdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_PagosClientes_IdMetodoPago",
                table: "PagosClientes",
                column: "IdMetodoPago");

            migrationBuilder.CreateIndex(
                name: "IX_PagosClientes_IdUsuario",
                table: "PagosClientes",
                column: "IdUsuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PagoClienteFacturas");

            migrationBuilder.DropTable(
                name: "PagosClientes");

            migrationBuilder.AddColumn<decimal>(
                name: "SaldoPendiente",
                table: "Facturas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
