using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using QuestPDF.Elements;
using System.Collections.Generic;
using SistemaTickets.Models;

namespace SistemaTickets.Models
{

    public class ReporteTicketsPorCategoriaDocument : IDocument
    {
        public List<TicketsPorCategoriaViewModel> Tickets { get; }

        public ReporteTicketsPorCategoriaDocument(List<TicketsPorCategoriaViewModel> tickets)
        {
            Tickets = tickets;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12));

                page.Header().Text("Informe de Tickets por Categoría")
                    .SemiBold().FontSize(18).FontColor(Colors.Blue.Medium);

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(60); // ID
                        columns.RelativeColumn();   // Nombre
                        columns.ConstantColumn(100); // Cantidad
                    });

                    // Encabezados
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("ID");
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Categoría");
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Cantidad");
                    });

                    // Filas
                    foreach (var item in Tickets)
                    {
                        table.Cell().Padding(5).Text(item.CategoriaId.ToString());
                        table.Cell().Padding(5).Text(item.NombreCategoria);
                        table.Cell().Padding(5).Text(item.Cantidad.ToString());
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Generado el: ").FontColor(Colors.Grey.Medium);
                    text.Span(DateTime.Now.ToString("dd/MM/yyyy")).FontColor(Colors.Grey.Medium);
                });
            });
        }
    }






}
