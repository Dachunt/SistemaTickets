using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SistemaTickets.Models;

namespace SistemaTickets.Controllers
{
    public class DashboardController : Controller
    {
        // GET: GraficosAdminController
        private readonly SistemaTicketsContext _context;

        public DashboardController(SistemaTicketsContext context)
        {
            _context = context;
        }


        public IActionResult Index()
        {
            var fechaInicio = DateTime.Now.AddDays(-30);
            var fechaFin = DateTime.Now;

            var tickets = _context.Tickets
                .Where(t => t.FechaCreacion >= fechaInicio && t.FechaCreacion <= fechaFin)
                .ToList();

            // Calculamos los tiempos de resolución (en horas, por ejemplo)
            var tiemposResolucion = tickets
                .Where(t => t.Estado == "Resuelto" && t.FechaResolucion.HasValue)
                .Select(t => new
                {
                    TicketId = t.TicketId,
                    TiempoResolucionHoras = (t.FechaResolucion.Value - t.FechaCreacion).TotalHours
                })
                .ToList();

            var model = new DashboardViewModel
            {
                TotalAbiertos = tickets.Count(t => t.Estado == "Abierto"),
                TotalEnProgreso = tickets.Count(t => t.Estado == "En Progreso"),
                TotalResueltos = tickets.Count(t => t.Estado == "Resuelto"),

                TicketsPorCategoria = tickets
                    .GroupBy(t => t.CategoriaId.ToString())
                    .ToDictionary(g => g.Key, g => g.Count()),

                TicketsPorUsuario = tickets
                    .GroupBy(t => t.UserId.ToString())
                    .ToDictionary(g => g.Key, g => g.Count()),

                TicketsPorDiaSemana = tickets
                    .GroupBy(t => t.FechaCreacion.DayOfWeek.ToString())
                    .ToDictionary(g => g.Key, g => g.Count()),

                PromedioTicketsDiarios = Math.Round(tickets.Count / 30.0, 2),
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,

                // Agregamos los tiempos de resolución al modelo para graficar
                TiemposResolucionHoras = tiemposResolucion
                    .ToDictionary(x => x.TicketId, x => x.TiempoResolucionHoras)
            };

            return View(model);
        }

    }
}
