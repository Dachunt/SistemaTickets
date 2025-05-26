using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using SistemaTickets.Models; // Ajusta el namespace según tu proyecto

public class ReporteTicketsPorUsuarioDocument : IDocument
{
    public List<TicketsPorUsuarioViewModel> Datos { get; }

    public ReporteTicketsPorUsuarioDocument(List<TicketsPorUsuarioViewModel> datos)
    {
        Datos = datos;
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

            // Título
            page.Header().Text("Resumen de Tickets por Usuario")
                .SemiBold().FontSize(18).FontColor(Colors.Blue.Medium);

            // Tabla
            page.Content().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                // Encabezado
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Usuario ID");
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Cantidad de Tickets");
                });

                // Filas
                foreach (var item in Datos)
                {
                    table.Cell().Padding(5).Text(item.UserId.ToString());
                    table.Cell().Padding(5).Text(item.Cantidad.ToString());
                }
            });

            // Pie de página
            page.Footer().AlignCenter().Text(text =>
            {
                text.Span("Generado el: ").FontColor(Colors.Grey.Medium);
                text.Span(DateTime.Now.ToString("dd/MM/yyyy")).FontColor(Colors.Grey.Medium);
            });
        });
    }
}
