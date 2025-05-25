using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaTickets.Models;

namespace SistemaTickets.Controllers
{
    public class SoporteTecnicoController : Controller
    {

        private readonly SistemaTicketsContext _context;

        public SoporteTecnicoController(SistemaTicketsContext context)
        {
            _context = context;
        }
        public IActionResult Home()
        {
            return View();
        }


        public async Task<IActionResult> Index()
        {
            // 1. Tickets resueltos (estado "Cerrado")
            var ticketsResueltos = await _context.Tickets
                .Where(t => t.Estado == "Cerrado")
                .ToListAsync();

            // 2. Tiempo promedio de resolución (en días) con JOIN entre HistorialEstados y Tickets
            var tiemposResolucion = await (
                from h in _context.HistorialEstados
                join t in _context.Tickets on h.TicketId equals t.TicketId
                where h.EstadoNuevo == "Cerrado"
                select EF.Functions.DateDiffDay(t.FechaCreacion, h.FechaCambio)
            ).ToListAsync();

            double promedioDias = tiemposResolucion.Any() ? tiemposResolucion.Average() : 0;

            // 3. Tickets en proceso (JOIN Asignaciones + Tickets)
            var ticketsEnProceso = await (
                from a in _context.Asignaciones
                join t in _context.Tickets on a.TicketId equals t.TicketId
                where t.Estado != "Cerrado"
                select t
            ).CountAsync();

            // 4. Tickets por mes (gráfica de barras)
            var ticketsPorMes = await (
                from t in _context.Tickets
                group t by new { t.FechaCreacion.Year, t.FechaCreacion.Month } into g
                select new
                {
                    Mes = g.Key.Month,
                    Año = g.Key.Year,
                    Cantidad = g.Count()
                }
            ).ToListAsync();

            // 5. Categorías (JOIN Tickets + Categorias)
            var categorias = await (
                from t in _context.Tickets
                join c in _context.Categorias on t.CategoriaId equals c.CategoriaId
                group t by c.Nombre into g
                select new
                {
                    Categoria = g.Key,
                    Cantidad = g.Count()
                }
            ).ToListAsync();

            // Enviar a la vista
            ViewBag.TiempoPromedio = promedioDias;
            ViewBag.EnProceso = ticketsEnProceso;
            ViewBag.Resueltos = ticketsResueltos.Count;
            ViewBag.TicketsPorMes = ticketsPorMes;
            ViewBag.Categorias = categorias;

            return View();
        }

    }
}
