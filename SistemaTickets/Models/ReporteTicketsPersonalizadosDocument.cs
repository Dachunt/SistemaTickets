using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using SistemaTickets.Models; // Ajusta según tu proyecto

public class ReporteTicketsPersonalizadosDocument : IDocument
{
    public List<TicketsPersonalizadoViewModel> Tickets { get; }

    public ReporteTicketsPersonalizadosDocument(List<TicketsPersonalizadoViewModel> tickets)
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
            page.DefaultTextStyle(x => x.FontSize(11));

            // Título
            page.Header().Text("Reporte Personalizado de Tickets")
                .SemiBold().FontSize(18).FontColor(Colors.Blue.Medium);

            // Tabla
            page.Content().Table(table =>
            {
                // Definición de columnas (puedes ajustar tamaños)
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(40);     // ID
                    columns.RelativeColumn(2);      // Usuario
                    columns.RelativeColumn(2);      // Categoría
                    columns.RelativeColumn(2);      // Fecha Creación
                    columns.RelativeColumn(1);      // Estado
                });

                // Encabezado
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("ID").SemiBold();
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Usuario").SemiBold();
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Categoría").SemiBold();
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Fecha Creación").SemiBold();
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Estado").SemiBold();
                });

                // Filas con datos
                foreach (var ticket in Tickets)
                {
                    table.Cell().Padding(5).Text(ticket.TicketId.ToString());

                    // Ajuste para usar las propiedades correctas del modelo TicketsPersonalizadoViewModel
                    var usuario = ticket.UsuarioNombre ?? "Desconocido";
                    var categoria = ticket.CategoriaNombre ?? "Desconocido";

                    table.Cell().Padding(5).Text(usuario);
                    table.Cell().Padding(5).Text(categoria);
                    table.Cell().Padding(5).Text(ticket.FechaCreacion.ToString("dd/MM/yyyy"));
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
