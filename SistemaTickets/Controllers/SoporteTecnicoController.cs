using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using SistemaTickets.Models;
using System.Security.Claims;

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
            ViewBag.MensajeExito = TempData["MensajeExito"] as string;
            return View();
        }

        public async Task<IActionResult> MisAsignaciones(string nombre, string prioridad, DateTime? fecha)
        {
            var userId = HttpContext.Session.GetInt32("id_usuario");
            if (userId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            var query = from a in _context.Asignaciones
                        join t in _context.Tickets on a.TicketId equals t.TicketId
                        join u in _context.Usuarios on t.UserId equals u.UserId
                        join c in _context.Categorias on t.CategoriaId equals c.CategoriaId
                        where a.TecnicoId == userId
                        select new
                        {
                            a.AsignacionId,
                            t.TicketId,
                            t.NombreAplicacion,
                            t.Estado,
                            t.Prioridad,
                            t.FechaCreacion,
                            t.FechaResolucion,
                            UsuarioNombre = u.Nombre,
                            Categoria = c.Nombre
                        };

            if (!string.IsNullOrEmpty(nombre))
                query = query.Where(x => x.NombreAplicacion == nombre);

            if (!string.IsNullOrEmpty(prioridad))
                query = query.Where(x => x.Prioridad == prioridad);

            if (fecha.HasValue)
                query = query.Where(x => x.FechaCreacion.Date == fecha.Value.Date);

            var asignaciones = await query.ToListAsync();

            ViewBag.Asignaciones = asignaciones;

            // Filtros disponibles
            ViewBag.NombresDisponibles = await _context.Tickets
                .Where(t => _context.Asignaciones.Any(a => a.TicketId == t.TicketId && a.TecnicoId == userId))
                .Select(t => t.NombreAplicacion)
                .Distinct()
                .ToListAsync();

            ViewBag.NombreFiltro = nombre;
            ViewBag.PrioridadFiltro = prioridad;
            ViewBag.FechaFiltro = fecha?.ToString("yyyy-MM-dd");

            return View();
        }


        public async Task<IActionResult> DetalleTicket(int id)
        {
            var ticket = await (from t in _context.Tickets
                                join u in _context.Usuarios on t.UserId equals u.UserId
                                join c in _context.Categorias on t.CategoriaId equals c.CategoriaId
                                where t.TicketId == id
                                select new
                                {
                                    t.TicketId,
                                    t.NombreAplicacion,
                                    t.Descripcion,
                                    t.Prioridad,
                                    t.Estado,
                                    t.FechaCreacion,
                                    t.FechaResolucion,
                                    UsuarioNombre = u.Nombre,
                                    UsuarioEmail = u.Email,
                                    CategoriaNombre = c.Nombre
                                }).FirstOrDefaultAsync();

            if (ticket == null)
                return NotFound();

            var archivos = await _context.ArchivosAdjuntos
                .Where(a => a.TicketId == id)
                .ToListAsync();

            var comentarios = await (from com in _context.Comentarios
                                     join u in _context.Usuarios on com.AutorId equals u.UserId
                                     where com.TicketId == id
                                     orderby com.FechaComentario descending
                                     select new
                                     {
                                         com.ComentarioId,
                                         com.Comentario,
                                         com.FechaComentario,
                                         Autor = u.Nombre
                                     }).ToListAsync();

            ViewBag.Ticket = ticket;
            ViewBag.Archivos = archivos;
            ViewBag.Comentarios = comentarios;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EnviarComentario(int ticketId, string comentario)
        {
            var userId = HttpContext.Session.GetInt32("id_usuario");
            var userRol = HttpContext.Session.GetInt32("rol");

            var NombreRol = _context.Roles.FirstOrDefault(r => r.RolId == userRol);
            if (NombreRol == null)
            {
                return RedirectToAction("Login", "Login");

            }
            if (userId == null) return RedirectToAction("Login", "Login");


            var nuevoComentario = new Comentarios
            {
                TicketId = ticketId,
                AutorId = userId.Value,
                Comentario = comentario,
                FechaComentario = DateTime.Now
            };

            _context.Comentarios.Add(nuevoComentario);

            _context.HistorialEstados.Add(new HistorialEstados
            {
                TicketId = ticketId,
                EstadoAnterior = "Sin comentario",
                EstadoNuevo = "Comentario agregado",
                FechaCambio = DateTime.Now,
                CambiadoPor = userId.Value
            });

            await _context.SaveChangesAsync();

            return RedirectToAction("Home", "MisAsignaciones", new { id = ticketId });

        }



        public async Task<IActionResult> ActualizarEstado(int id)
        {
            var ticket = await (from t in _context.Tickets
                                join u in _context.Usuarios on t.UserId equals u.UserId
                                where t.TicketId == id
                                select new
                                {
                                    t.TicketId,
                                    t.NombreAplicacion,
                                    t.Estado,
                                    Usuario = u.Nombre
                                }).FirstOrDefaultAsync();

            var archivos = await _context.ArchivosAdjuntos
                .Where(a => a.TicketId == id)
                .ToListAsync();

            ViewBag.Ticket = ticket;
            ViewBag.Archivos = archivos;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarEstado(int ticketId, string nuevoEstado)
        {
            var userId = HttpContext.Session.GetInt32("id_usuario");
            if (userId == null) return RedirectToAction("Login", "Login");

            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return NotFound();

            string estadoAnterior = ticket.Estado;
            ticket.Estado = nuevoEstado;

            // Si se cierra el ticket, registrar la fecha de resolución
            if (nuevoEstado.ToLower() == "Resuelto")
            {
                ticket.FechaResolucion = DateTime.Now;
            }

            _context.HistorialEstados.Add(new HistorialEstados
            {
                TicketId = ticketId,
                EstadoAnterior = estadoAnterior,
                EstadoNuevo = nuevoEstado,
                FechaCambio = DateTime.Now,
                CambiadoPor = userId.Value
            });

            await _context.SaveChangesAsync();
            return RedirectToAction("MisAsignaciones");
        }

        //Este es el de mis asignaciones
        public async Task<IActionResult> VerHistorial(int id, string nombre = "", string prioridad = "", string estado = "", DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            var userId = HttpContext.Session.GetInt32("id_usuario");

            // Obtener historial con filtros aplicados
            var historialQuery = from h in _context.HistorialEstados
                                 join u in _context.Usuarios on h.CambiadoPor equals u.UserId
                                 join t in _context.Tickets on h.TicketId equals t.TicketId
                                 where h.TicketId == id && h.CambiadoPor == userId
                                 select new
                                 {
                                     h.EstadoAnterior,
                                     h.EstadoNuevo,
                                     h.FechaCambio,
                                     Tecnico = u.Nombre,
                                     TicketPrioridad = t.Prioridad,
                                     TicketEstado = t.Estado
                                 };

            if (!string.IsNullOrEmpty(nombre))
                historialQuery = historialQuery.Where(h => h.Tecnico.Contains(nombre));

            if (!string.IsNullOrEmpty(prioridad))
                historialQuery = historialQuery.Where(h => h.TicketPrioridad == prioridad);

            if (!string.IsNullOrEmpty(estado))
                historialQuery = historialQuery.Where(h => h.EstadoNuevo == estado);

            if (fechaInicio.HasValue)
                historialQuery = historialQuery.Where(h => h.FechaCambio >= fechaInicio.Value);

            if (fechaFin.HasValue)
                historialQuery = historialQuery.Where(h => h.FechaCambio <= fechaFin.Value);

            var historial = await historialQuery
                .OrderByDescending(h => h.FechaCambio)
                .ToListAsync();

            ViewBag.Historial = historial;
            ViewBag.NombreFiltro = nombre;
            ViewBag.PrioridadFiltro = prioridad;
            ViewBag.EstadoFiltro = estado;
            ViewBag.FechaInicio = fechaInicio?.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin?.ToString("yyyy-MM-dd");

            return View();
        }

        public async Task<IActionResult> cerrarTickets(string nombre = "", string prioridad = "", DateTime? fecha = null)
        {
            var userId = HttpContext.Session.GetInt32("id_usuario");

            // Consulta de asignaciones
            var asignacionesQuery = from a in _context.Asignaciones
                                    join t in _context.Tickets on a.TicketId equals t.TicketId
                                    join u in _context.Usuarios on t.UserId equals u.UserId
                                    join c in _context.Categorias on t.CategoriaId equals c.CategoriaId
                                    where a.TecnicoId == userId
                                    select new
                                    {
                                        t.TicketId,
                                        NombreAplicacion = t.NombreAplicacion, 
                                        Categoria = c.Nombre,
                                        UsuarioNombre = u.Nombre,
                                        Prioridad = t.Prioridad,
                                        FechaCreacion = t.FechaCreacion,
                                        Estado = t.Estado
                                    };

            if (!string.IsNullOrEmpty(nombre))
                asignacionesQuery = asignacionesQuery.Where(a => a.NombreAplicacion.Contains(nombre));

            if (!string.IsNullOrEmpty(prioridad))
                asignacionesQuery = asignacionesQuery.Where(a => a.Prioridad == prioridad);

            if (fecha.HasValue)
                asignacionesQuery = asignacionesQuery.Where(a => a.FechaCreacion.Date == fecha.Value.Date);

            var asignaciones = await asignacionesQuery.ToListAsync();

            // Obtener nombres únicos de aplicaciones para el filtro
            var nombresDisponibles = await _context.Tickets
                .Select(t => t.NombreAplicacion)
                .Distinct()
                .ToListAsync();

            // Cargar en ViewBag lo que la vista necesita
            ViewBag.Asignaciones = asignaciones;
            ViewBag.NombresDisponibles = nombresDisponibles;
            ViewBag.NombreFiltro = nombre;
            ViewBag.PrioridadFiltro = prioridad;
            ViewBag.FechaFiltro = fecha?.ToString("yyyy-MM-dd");

            return View();
        }

        // GET: Mostrar detalle y formulario para cerrar ticket
        public async Task<IActionResult> VerDetalleYCerrarTicket(int id)
        {
            var ticket = await (from t in _context.Tickets
                                join u in _context.Usuarios on t.UserId equals u.UserId
                                join c in _context.Categorias on t.CategoriaId equals c.CategoriaId
                                where t.TicketId == id
                                select new
                                {
                                    t.TicketId,
                                    t.NombreAplicacion,
                                    t.Descripcion,
                                    t.Prioridad,
                                    t.Estado,
                                    t.FechaCreacion,
                                    t.FechaResolucion,
                                    UsuarioNombre = u.Nombre,
                                    UsuarioEmail = u.Email,
                                    CategoriaNombre = c.Nombre
                                }).FirstOrDefaultAsync();

            if (ticket == null)
                return NotFound();

            var archivos = await _context.ArchivosAdjuntos
                .Where(a => a.TicketId == id)
                .ToListAsync();

            var comentarios = await (from com in _context.Comentarios
                                     join u in _context.Usuarios on com.AutorId equals u.UserId
                                     where com.TicketId == id
                                     orderby com.FechaComentario descending
                                     select new
                                     {
                                         com.ComentarioId,
                                         com.Comentario,
                                         com.FechaComentario,
                                         Autor = u.Nombre
                                     }).ToListAsync();

            ViewBag.Ticket = ticket;
            ViewBag.Archivos = archivos;
            ViewBag.Comentarios = comentarios;

            return View();
        }

        // POST: Recibe comentario y cierra el ticket
        [HttpPost]
        public async Task<IActionResult> VerDetalleYCerrarTicket(int ticketId, string comentario)
        {
            var userId = HttpContext.Session.GetInt32("id_usuario");

            if (userId == null) return RedirectToAction("Login", "Login");

            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return NotFound();

            // Agregar comentario
            if (!string.IsNullOrWhiteSpace(comentario))
            {
                var nuevoComentario = new Comentarios
                {
                    TicketId = ticketId,
                    AutorId = userId.Value,
                    Comentario = comentario,
                    FechaComentario = DateTime.Now
                };
                _context.Comentarios.Add(nuevoComentario);
            }

            // Cambiar estado a "Resuelto"
            string estadoAnterior = ticket.Estado;
            ticket.Estado = "Resuelto";
            ticket.FechaResolucion = DateTime.Now;

            // Agregar registro en historial
            var historial = new HistorialEstados
            {
                TicketId = ticketId,
                EstadoAnterior = estadoAnterior,
                EstadoNuevo = "Resuelto",
                FechaCambio = DateTime.Now,
                CambiadoPor = userId.Value
            };
            _context.HistorialEstados.Add(historial);

            await _context.SaveChangesAsync();

            // Redirigir a la lista o detalle
            return RedirectToAction("VerHistorial", new { id = ticketId });
        }


        public async Task<IActionResult> Index()
        {
            var ticketsResueltos = await _context.Tickets
                .Where(t => t.Estado == "Resuelto")
                .ToListAsync();

            var tiemposResolucion = await (
                from h in _context.HistorialEstados
                join t in _context.Tickets on h.TicketId equals t.TicketId
                where h.EstadoNuevo == "Resuelto"
                select EF.Functions.DateDiffDay(t.FechaCreacion, h.FechaCambio)
            ).ToListAsync();

            double promedioDias = tiemposResolucion.Any() ? tiemposResolucion.Average() : 0;

            var ticketsEnProceso = await (
                from a in _context.Asignaciones
                join t in _context.Tickets on a.TicketId equals t.TicketId
                where t.Estado != "Resuelto"
                select t
            ).CountAsync();

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

            ViewBag.TiempoPromedio = promedioDias;
            ViewBag.EnProceso = ticketsEnProceso;
            ViewBag.Resueltos = ticketsResueltos.Count;
            ViewBag.TicketsPorMes = ticketsPorMes;
            ViewBag.Categorias = categorias;

            return View();
        }

        public async Task<IActionResult> InformeTecnico(string nombre, string prioridad, DateTime? fecha)
        {
            var userId = HttpContext.Session.GetInt32("id_usuario");
            if (userId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            var query = from a in _context.Asignaciones
                        join t in _context.Tickets on a.TicketId equals t.TicketId
                        join u in _context.Usuarios on t.UserId equals u.UserId
                        join c in _context.Categorias on t.CategoriaId equals c.CategoriaId
                        where a.TecnicoId == userId
                        select new
                        {
                            a.AsignacionId,
                            t.TicketId,
                            t.NombreAplicacion,
                            t.Estado,
                            t.Prioridad,
                            t.FechaCreacion,
                            t.FechaResolucion,
                            UsuarioNombre = u.Nombre,
                            Categoria = c.Nombre
                        };

            if (!string.IsNullOrEmpty(nombre))
                query = query.Where(x => x.NombreAplicacion == nombre);

            if (!string.IsNullOrEmpty(prioridad))
                query = query.Where(x => x.Prioridad == prioridad);

            if (fecha.HasValue)
                query = query.Where(x => x.FechaCreacion.Date == fecha.Value.Date);

            var asignaciones = await query.ToListAsync();

            ViewBag.Asignaciones = asignaciones;

            // Filtros disponibles
            ViewBag.NombresDisponibles = await _context.Tickets
                .Where(t => _context.Asignaciones.Any(a => a.TicketId == t.TicketId && a.TecnicoId == userId))
                .Select(t => t.NombreAplicacion)
                .Distinct()
                .ToListAsync();

            ViewBag.NombreFiltro = nombre;
            ViewBag.PrioridadFiltro = prioridad;
            ViewBag.FechaFiltro = fecha?.ToString("yyyy-MM-dd");

            return View();
        }


        public async Task<IActionResult> GenerarInformeTicket(int id)
        {
            var ticket = await (from t in _context.Tickets
                                join u in _context.Usuarios on t.UserId equals u.UserId
                                join c in _context.Categorias on t.CategoriaId equals c.CategoriaId
                                where t.TicketId == id
                                select new
                                {
                                    t.TicketId,
                                    t.NombreAplicacion,
                                    t.Descripcion,
                                    t.Prioridad,
                                    t.Estado,
                                    t.FechaCreacion,
                                    t.FechaResolucion,
                                    Usuario = u.Nombre,
                                    UsuarioEmail = u.Email,
                                    Categoria = c.Nombre
                                }).FirstOrDefaultAsync();

            if (ticket == null)
                return NotFound();

            var archivos = await _context.ArchivosAdjuntos
                .Where(a => a.TicketId == id)
                .Select(a => a.NombreArchivo)
                .ToListAsync();

            var comentarios = await (from com in _context.Comentarios
                                     join u in _context.Usuarios on com.AutorId equals u.UserId
                                     where com.TicketId == id
                                     orderby com.FechaComentario
                                     select new
                                     {
                                         com.Comentario,
                                         com.FechaComentario,
                                         Autor = u.Nombre
                                     }).ToListAsync();

            var historial = await (from h in _context.HistorialEstados
                                   join u in _context.Usuarios on h.CambiadoPor equals u.UserId
                                   where h.TicketId == id
                                   orderby h.FechaCambio
                                   select new
                                   {
                                       h.EstadoAnterior,
                                       h.EstadoNuevo,
                                       h.FechaCambio,
                                       Tecnico = u.Nombre
                                   }).ToListAsync();

            
            var pdfBytes = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Header().Text($"Informe de Ticket #{ticket.TicketId}").FontSize(20).Bold();
                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Aplicación: {ticket.NombreAplicacion}");
                        col.Item().Text($"Descripción: {ticket.Descripcion}");
                        col.Item().Text($"Prioridad: {ticket.Prioridad}");
                        col.Item().Text($"Estado: {ticket.Estado}");
                        col.Item().Text($"Fecha de Creación: {ticket.FechaCreacion:dd/MM/yyyy}");
                        col.Item().Text($"Fecha de Resolución: {(ticket.FechaResolucion != null ? ticket.FechaResolucion.Value.ToString("dd/MM/yyyy") : "Pendiente")}");
                        col.Item().Text($"Usuario: {ticket.Usuario} ({ticket.UsuarioEmail})");
                        col.Item().Text($"Categoría: {ticket.Categoria}");

                        col.Item().PaddingVertical(10).Text("Archivos Adjuntos:").Bold();
                        if (archivos.Any())
                        {
                            foreach (var archivo in archivos)
                                col.Item().Text($"- {archivo}");
                        }
                        else
                        {
                            col.Item().Text("No hay archivos adjuntos.");
                        }

                        col.Item().PaddingVertical(10).Text("Comentarios:").Bold();
                        if (comentarios.Any())
                        {
                            foreach (var c in comentarios)
                                col.Item().Text($"{c.FechaComentario:dd/MM/yyyy HH:mm} - {c.Autor}: {c.Comentario}");
                        }
                        else
                        {
                            col.Item().Text("No hay comentarios.");
                        }

                        col.Item().PaddingVertical(10).Text("Historial de Estados:").Bold();
                        if (historial.Any())
                        {
                            foreach (var h in historial)
                                col.Item().Text($"{h.FechaCambio:dd/MM/yyyy HH:mm} - {h.Tecnico}: {h.EstadoAnterior} → {h.EstadoNuevo}");
                        }
                        else
                        {
                            col.Item().Text("No hay historial de estados.");
                        }
                    });
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Generado por Sistema de Tickets - ");
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", $"Ticket_{ticket.TicketId}_Informe.pdf");
        }


    }
}
