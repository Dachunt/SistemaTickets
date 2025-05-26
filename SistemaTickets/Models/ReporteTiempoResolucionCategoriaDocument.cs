using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using SistemaTickets.Models; // Ajusta el namespace

public class ReporteTiempoResolucionCategoriaDocument : IDocument
{
    public List<InformeTiempoResolucionCategoria> Datos { get; }

    public ReporteTiempoResolucionCategoriaDocument(List<InformeTiempoResolucionCategoria> datos)
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
            page.Header().Text("Promedio de Resolución por Categoría")
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
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Categoría ID");
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Promedio (Horas)");
                });

                // Filas
                foreach (var item in Datos)
                {
                    table.Cell().Padding(5).Text(item.CategoriaId.ToString());
                    table.Cell().Padding(5).Text($"{item.PromedioHoras:F2}");
                }
            });

            // Pie
            page.Footer().AlignCenter().Text(text =>
            {
                text.Span("Generado el: ").FontColor(Colors.Grey.Medium);
                text.Span(DateTime.Now.ToString("dd/MM/yyyy")).FontColor(Colors.Grey.Medium);
            });
        });
    }
}
