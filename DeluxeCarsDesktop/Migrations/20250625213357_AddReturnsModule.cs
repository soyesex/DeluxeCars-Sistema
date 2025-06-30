using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeluxeCarsDesktop.Migrations
{
    /// <inheritdoc />
    public partial class AddReturnsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotasDeCredito",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroNota = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MontoTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IdFacturaOriginal = table.Column<int>(type: "int", nullable: false),
                    IdCliente = table.Column<int>(type: "int", nullable: false),
                    IdUsuario = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotasDeCredito", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotasDeCredito_Clientes_IdCliente",
                        column: x => x.IdCliente,
                        principalTable: "Clientes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NotasDeCredito_Facturas_IdFacturaOriginal",
                        column: x => x.IdFacturaOriginal,
                        principalTable: "Facturas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NotasDeCredito_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DetallesNotaDeCredito",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdNotaDeCredito = table.Column<int>(type: "int", nullable: false),
                    IdProducto = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReingresaAInventario = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesNotaDeCredito", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesNotaDeCredito_NotasDeCredito_IdNotaDeCredito",
                        column: x => x.IdNotaDeCredito,
                        principalTable: "NotasDeCredito",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetallesNotaDeCredito_Productos_IdProducto",
                        column: x => x.IdProducto,
                        principalTable: "Productos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetallesNotaDeCredito_IdNotaDeCredito",
                table: "DetallesNotaDeCredito",
                column: "IdNotaDeCredito");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesNotaDeCredito_IdProducto",
                table: "DetallesNotaDeCredito",
                column: "IdProducto");

            migrationBuilder.CreateIndex(
                name: "IX_NotasDeCredito_IdCliente",
                table: "NotasDeCredito",
                column: "IdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_NotasDeCredito_IdFacturaOriginal",
                table: "NotasDeCredito",
                column: "IdFacturaOriginal");

            migrationBuilder.CreateIndex(
                name: "IX_NotasDeCredito_IdUsuario",
                table: "NotasDeCredito",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_NotasDeCredito_NumeroNota",
                table: "NotasDeCredito",
                column: "NumeroNota",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetallesNotaDeCredito");

            migrationBuilder.DropTable(
                name: "NotasDeCredito");
        }
    }
}
