using Microsoft.AspNetCore.Mvc;
using SistemaTickets.Models;
using System;
using System.Linq;

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
                    Cantidad = g.Count()
                })
                .ToList();

            return View(resultado);
        }


        public IActionResult TicketsPorRangoFechas(DateTime? inicio, DateTime? fin)
        {
            var fechaInicio = inicio ?? DateTime.Now.AddDays(-30);
            var fechaFin = fin ?? DateTime.Now;

            var datos = _context.Tickets
                .Where(t => t.FechaCreacion >= fechaInicio && t.FechaCreacion <= fechaFin)
                .ToList();

            ViewBag.FechaInicio = fechaInicio;
            ViewBag.FechaFin = fechaFin;

            return View(datos);
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


    }
}

