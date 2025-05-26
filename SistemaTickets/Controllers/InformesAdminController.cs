using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using SistemaTickets.Models;
using System;
using System.Linq;
using QuestPDF.Fluent;

namespace SistemaTickets.Controllers
{
    public class InformesAdminController : Controller
    {
        private readonly SistemaTicketsContext _context;

        public InformesAdminController(SistemaTicketsContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult TicketsPorCategoria()
        {
            var resultado = _context.Tickets
                .GroupBy(t => t.CategoriaId)
                .Select(g => new TicketsPorCategoriaViewModel
                {
                    CategoriaId = g.Key,
                    NombreCategoria = _context.Categorias
                        .Where(c => c.CategoriaId == g.Key)
                        .Select(c => c.Nombre)
                        .FirstOrDefault(),
                    Cantidad = g.Count()
                })
                .ToList();

            return View(resultado);
        }

        public IActionResult DescargarPdf()
        {
            var datos = _context.Tickets
                .GroupBy(t => t.CategoriaId)
                .Select(g => new TicketsPorCategoriaViewModel
                {
                    CategoriaId = g.Key,
                    NombreCategoria = _context.Categorias
                        .Where(c => c.CategoriaId == g.Key)
                        .Select(c => c.Nombre)
                        .FirstOrDefault(),
                    Cantidad = g.Count()
                })
                .ToList();

            var documento = new ReporteTicketsPorCategoriaDocument(datos);
            var pdfBytes = documento.GeneratePdf();

            return File(pdfBytes, "application/pdf", "TicketsPorCategoria.pdf");
        }

        public IActionResult TicketsPorRango(DateTime fechaInicio, DateTime fechaFin)
        {
            var tickets = _context.Tickets
                .Where(t => t.FechaCreacion >= fechaInicio && t.FechaCreacion <= fechaFin)
                .ToList();

            ViewBag.FechaInicio = fechaInicio;
            ViewBag.FechaFin = fechaFin;

            return View("TicketsPorRangoFechas", tickets); // Usa el nombre exacto del archivo .cshtml

        }



        public IActionResult TicketsPorRangoFechas(DateTime? fechaInicio, DateTime? fechaFin)
        {
            if (!fechaInicio.HasValue || !fechaFin.HasValue)
            {
                fechaInicio = DateTime.Today.AddDays(-7);
                fechaFin = DateTime.Today;
            }

            var tickets = _context.Tickets
                .Where(t => t.FechaCreacion >= fechaInicio && t.FechaCreacion <= fechaFin)
                .ToList();

            ViewBag.FechaInicio = fechaInicio;
            ViewBag.FechaFin = fechaFin;

            return View(tickets);
        }


        public IActionResult DescargarPdfRango(DateTime fechaInicio, DateTime fechaFin)
        {
            var tickets = _context.Tickets
                .Where(t => t.FechaCreacion >= fechaInicio && t.FechaCreacion <= fechaFin)
                .ToList();

            var documento = new ReporteTicketsRangoFechasDocument(tickets, fechaInicio, fechaFin);
            var pdf = documento.GeneratePdf();

            return File(pdf, "application/pdf", $"Tickets_{fechaInicio:yyyyMMdd}_{fechaFin:yyyyMMdd}.pdf");
        }



        public IActionResult TicketsPorUsuario()
        {
            var resultado = _context.Tickets
                .Where(t => t.Estado == "Resuelto") // opcional, si aplica
                .GroupBy(t => t.UserId)
                .Select(g => new TicketsPorUsuarioViewModel
                {
                    UserId = g.Key,
                    Cantidad = g.Count()
                })
                .ToList();

            return View(resultado);
        }



        public IActionResult DescargarPdfUsuarios()
        {
            var datos = _context.Tickets
                .Where(t => t.Estado == "Resuelto") // o sin filtro si no aplica
                .GroupBy(t => t.UserId)
                .Select(g => new TicketsPorUsuarioViewModel
                {
                    UserId = g.Key,
                    Cantidad = g.Count()
                })
                .ToList();

            var documento = new ReporteTicketsPorUsuarioDocument(datos);
            var pdf = documento.GeneratePdf();

            return File(pdf, "application/pdf", $"TicketsPorUsuario_{DateTime.Now:yyyyMMdd}.pdf");
        }


        public IActionResult TiempoResolucionPorCategoria()
        {
            var datos = _context.Tickets
                .Where(t => t.Estado == "Resuelto" && t.FechaResolucion.HasValue)
                .ToList()
                .GroupBy(t => t.CategoriaId)
                .Select(g => new InformeTiempoResolucionCategoria
                {
                    CategoriaId = g.Key,
                    PromedioHoras = g.Average(t => (t.FechaResolucion.Value - t.FechaCreacion).TotalHours)
                })
                .ToList();

            return View(datos);
        }

        public IActionResult DescargarPdfPromedioResolucion()
        {
            var datos = _context.Tickets
                .Where(t => t.Estado == "Resuelto" && t.FechaResolucion.HasValue)
                .ToList()
                .GroupBy(t => t.CategoriaId)
                .Select(g => new InformeTiempoResolucionCategoria
                {
                    CategoriaId = g.Key,
                    PromedioHoras = g.Average(t => (t.FechaResolucion.Value - t.FechaCreacion).TotalHours)
                })
                .ToList();

            var documento = new ReporteTiempoResolucionCategoriaDocument(datos);
            var pdf = documento.GeneratePdf();

            return File(pdf, "application/pdf", $"PromedioResolucion_{DateTime.Now:yyyyMMdd}.pdf");
        }


        [HttpGet]
        public IActionResult GenerarReportePersonalizado(
           int? userId = null,
           DateTime? fechaInicio = null,
           DateTime? fechaFin = null,
           int? categoriaId = null)
        {
            var query = _context.Tickets.AsQueryable();

            if (userId.HasValue)
                query = query.Where(t => t.UserId == userId.Value);

            if (categoriaId.HasValue)
                query = query.Where(t => t.CategoriaId == categoriaId.Value);

            if (fechaInicio.HasValue)
                query = query.Where(t => t.FechaCreacion >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(t => t.FechaCreacion <= fechaFin.Value);

            var ticketsFiltrados = query
                .Join(_context.Usuarios, t => t.UserId, u => u.UserId, (t, u) => new { t, UsuarioNombre = u.Nombre })
                .Join(_context.Categorias, tu => tu.t.CategoriaId, c => c.CategoriaId, (tu, c) => new TicketsPersonalizadoViewModel
                {
                    TicketId = tu.t.TicketId,
                    UsuarioNombre = tu.UsuarioNombre,
                    CategoriaNombre = c.Nombre,
                    FechaCreacion = tu.t.FechaCreacion,
                    Estado = tu.t.Estado
                })
                .ToList();

            ViewBag.UserId = userId;
            ViewBag.FechaInicio = fechaInicio;
            ViewBag.FechaFin = fechaFin;
            ViewBag.CategoriaId = categoriaId;

            return View("ReportePersonalizado", ticketsFiltrados);
        }


        [HttpGet]
        public IActionResult DescargarReportePersonalizadoPdf(
            int? userId = null,
            DateTime? fechaInicio = null,
            DateTime? fechaFin = null,
            int? categoriaId = null)
        {
            var query = _context.Tickets.AsQueryable();

            if (userId.HasValue)
                query = query.Where(t => t.UserId == userId.Value);

            if (categoriaId.HasValue)
                query = query.Where(t => t.CategoriaId == categoriaId.Value);

            if (fechaInicio.HasValue)
                query = query.Where(t => t.FechaCreacion >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(t => t.FechaCreacion <= fechaFin.Value);

            var ticketsFiltrados = query
            .Join(_context.Usuarios, t => t.UserId, u => u.UserId, (t, u) => new { t, UsuarioNombre = u.Nombre })
            .Join(_context.Categorias, tu => tu.t.CategoriaId, c => c.CategoriaId, (tu, c) => new TicketsPersonalizadoViewModel
            {
                TicketId = tu.t.TicketId,
                UsuarioNombre = tu.UsuarioNombre,
                CategoriaNombre = c.Nombre,
                FechaCreacion = tu.t.FechaCreacion,
                Estado = tu.t.Estado
            })
            .ToList();


            var doc = new ReporteTicketsPersonalizadosDocument(ticketsFiltrados);
            var pdf = doc.GeneratePdf();

            return File(pdf, "application/pdf", $"ReportePersonalizado_{DateTime.Now:yyyyMMdd}.pdf");
        }







    }
}

