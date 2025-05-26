using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using SistemaTickets.Models;

namespace SistemaTickets.Models
{
    public class ReporteTicketsRangoFechasDocument : IDocument
    {
        public List<Tickets> Tickets { get; }
        public DateTime FechaInicio { get; }
        public DateTime FechaFin { get; }

        public ReporteTicketsRangoFechasDocument(List<Tickets> tickets, DateTime fechaInicio, DateTime fechaFin)
        {
            Tickets = tickets;
            FechaInicio = fechaInicio;
            FechaFin = fechaFin;
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
                page.Header().Text($"Informe de Tickets del {FechaInicio:dd/MM/yyyy} al {FechaFin:dd/MM/yyyy}")
                    .SemiBold().FontSize(18).FontColor(Colors.Blue.Medium);

                // Tabla de contenido
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(60); // ID
                        columns.RelativeColumn();   // Categoría
                        columns.RelativeColumn();   // Usuario
                        columns.ConstantColumn(120); // Fecha
                        columns.ConstantColumn(80); // Estado
                    });

                    // Encabezados
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("ID");
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Categoría");
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Usuario");
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Fecha");
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Estado");
                    });

                    // Filas
                    foreach (var ticket in Tickets)
                    {
                        table.Cell().Padding(5).Text(ticket.TicketId.ToString());
                        table.Cell().Padding(5).Text(ticket.CategoriaId.ToString());
                        table.Cell().Padding(5).Text(ticket.UserId.ToString());
                        table.Cell().Padding(5).Text(ticket.FechaCreacion.ToString("dd/MM/yyyy HH:mm"));
                        table.Cell().Padding(5).Text(ticket.Estado);
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
}
