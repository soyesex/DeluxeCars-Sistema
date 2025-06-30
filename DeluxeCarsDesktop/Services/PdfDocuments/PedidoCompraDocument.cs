using DeluxeCarsDesktop.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Services.PdfDocuments
{
    public class PedidoCompraDocument : IDocument
    {
        private readonly Pedido _pedido;

        public PedidoCompraDocument(Pedido pedido)
        {
            _pedido = pedido;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    // Definimos márgenes, tamaño de página, etc.
                    page.Margin(50);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Arial"));

                    // Definimos el Header (se repite en cada página)
                    page.Header().Element(ComposeHeader);

                    // Definimos el Contenido principal
                    page.Content().Element(ComposeContent);

                    // Definimos el Footer (se repite en cada página)
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
                });
        }

        void ComposeHeader(IContainer container)
        {
            var titleStyle = TextStyle.Default.FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);

            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text($"Orden de Compra N°: {_pedido.NumeroPedido}").Style(titleStyle);
                    column.Item().Text($"Fecha de Emisión: {_pedido.FechaEmision:dd/MM/yyyy}");
                    column.Item().Text($"Fecha Estimada de Entrega: {_pedido.FechaEstimadaEntrega:dd/MM/yyyy}");
                });

                // Aquí podrías poner el logo de tu empresa
                // row.ConstantItem(100).Height(50).Placeholder(); 
            });
        }

        void ComposeContent(IContainer container)
        {
            container.PaddingVertical(40).Column(column =>
            {
                // Datos del Proveedor
                column.Item().Element(ComposeSupplierInfo);
                column.Spacing(20); // Espacio

                // Tabla de Productos
                column.Item().Element(ComposeItemsTable);
                column.Spacing(10);

                // Total
                var total = _pedido.DetallesPedidos.Sum(d => d.Cantidad * d.PrecioUnitario);
                column.Item().AlignRight().Text($"Total Pedido: {total:C}").Bold();

                // Observaciones
                if (!string.IsNullOrWhiteSpace(_pedido.Observaciones))
                {
                    column.Item().PaddingTop(25).Text("Observaciones:").Bold();
                    column.Item().Text(_pedido.Observaciones);
                }
            });
        }

        void ComposeSupplierInfo(IContainer container)
        {
            container.ShowOnce().Column(column => // ShowOnce evita que se repita en páginas nuevas si hay un salto
            {
                column.Item().Text("Proveedor:").SemiBold();
                column.Item().PaddingLeft(10).Text(_pedido.Proveedor.RazonSocial);
                column.Item().PaddingLeft(10).Text($"NIT: {_pedido.Proveedor.NIT}");
                column.Item().PaddingLeft(10).Text($"Departamento: {_pedido.Proveedor.Municipio.Departamento.Nombre}");
                column.Item().PaddingLeft(10).Text($"Teléfono: {_pedido.Proveedor.Telefono}");
            });
        }

        void ComposeItemsTable(IContainer container)
        {
            container.Table(table =>
            {
                // Definimos las columnas de la tabla
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3); // Nombre del producto
                    columns.RelativeColumn(1); // Cantidad
                    columns.RelativeColumn(2); // Precio Unitario
                    columns.RelativeColumn(2); // Subtotal
                });

                // Definimos el encabezado de la tabla
                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("Producto");
                    header.Cell().Element(CellStyle).AlignRight().Text("Cantidad");
                    header.Cell().Element(CellStyle).AlignRight().Text("Precio Unitario");
                    header.Cell().Element(CellStyle).AlignRight().Text("Subtotal");

                    static IContainer CellStyle(IContainer c) => c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5);
                });

                // Llenamos la tabla con los productos del pedido
                foreach (var item in _pedido.DetallesPedidos)
                {
                    table.Cell().Element(CellStyle).Text(item.Descripcion);
                    table.Cell().Element(CellStyle).AlignRight().Text(item.Cantidad.ToString());
                    table.Cell().Element(CellStyle).AlignRight().Text($"{item.PrecioUnitario:C}");
                    table.Cell().Element(CellStyle).AlignRight().Text($"{(item.Cantidad * item.PrecioUnitario):C}");

                    static IContainer CellStyle(IContainer c) => c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5);
                }
            });
        }
    }
}
