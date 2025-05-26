using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
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

        public async Task<IActionResult> VerInforme(int id)
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
                .ToListAsync();

            var comentarios = await (from com in _context.Comentarios
                                     join u in _context.Usuarios on com.AutorId equals u.UserId
                                     where com.TicketId == id
                                     orderby com.FechaComentario
                                     select new
                                     {
                                         com.ComentarioId,
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

            ViewBag.Ticket = ticket;
            ViewBag.Archivos = archivos;
            ViewBag.Comentarios = comentarios;
            ViewBag.Historial = historial;

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

        public async Task<IActionResult> VerTicket(int id)
        {
            var userId = HttpContext.Session.GetInt32("id_usuario");
            if (userId == null)
            {
                return RedirectToAction("Login", "Login");
            }

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
                                    Categoria = c.Nombre,
                                    EmailUsuario = u.Email
                                }).FirstOrDefaultAsync();

            if (ticket == null)
                return NotFound();

            var archivos = await _context.ArchivosAdjuntos
                                .Where(a => a.TicketId == id)
                                .ToListAsync();

            var comentarios = await (from c in _context.Comentarios
                                     join u in _context.Usuarios on c.AutorId equals u.UserId
                                     where c.TicketId == id
                                     orderby c.FechaComentario
                                     select new
                                     {
                                         Autor = u.Nombre,
                                         Comentario = c.Comentario,
                                         Fecha = c.FechaComentario
                                     }).ToListAsync();

            ViewBag.Ticket = ticket;
            ViewBag.Archivos = archivos;
            ViewBag.Comentarios = comentarios;

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
            return RedirectToAction("Home", "SoporteTecnico", new { id = ticketId });
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


        //Este es el general
        public async Task<IActionResult> Details(
        string nombre = "",
        string prioridad = "",
        string estado = "",
        DateTime? fechaInicio = null,
        DateTime? fechaFin = null,
        DateTime? resolucionInicio = null,
        DateTime? resolucionFin = null)
        {
            var historial = await (from a in _context.Asignaciones
                                   join t in _context.Tickets on a.TicketId equals t.TicketId
                                   join u in _context.Usuarios on t.UserId equals u.UserId
                                   join c in _context.Categorias on t.CategoriaId equals c.CategoriaId
                                   where t.Estado == "Resuelto"
                                   select new
                                   {
                                       a.AsignacionId,
                                       t.TicketId,
                                       u.Nombre,
                                       t.NombreAplicacion,
                                       t.Prioridad,
                                       t.Estado,
                                       t.FechaCreacion,
                                       t.FechaResolucion
                                   }).ToListAsync();

            if (!string.IsNullOrEmpty(nombre))
                historial = historial.Where(h => h.NombreAplicacion.Contains(nombre)).ToList();

            if (!string.IsNullOrEmpty(prioridad))
                historial = historial.Where(h => h.Prioridad == prioridad).ToList();

            if (!string.IsNullOrEmpty(estado))
                historial = historial.Where(h => h.Estado == estado).ToList();

            if (fechaInicio.HasValue)
                historial = historial.Where(h => h.FechaCreacion >= fechaInicio.Value).ToList();

            if (fechaFin.HasValue)
                historial = historial.Where(h => h.FechaCreacion <= fechaFin.Value).ToList();

            if (resolucionInicio.HasValue)
                historial = historial.Where(h => h.FechaResolucion.HasValue && h.FechaResolucion >= resolucionInicio.Value).ToList();

            if (resolucionFin.HasValue)
                historial = historial.Where(h => h.FechaResolucion.HasValue && h.FechaResolucion <= resolucionFin.Value).ToList();

            ViewBag.NombreFiltro = nombre;
            ViewBag.PrioridadFiltro = prioridad;
            ViewBag.EstadoFiltro = estado;
            ViewBag.FechaInicioFiltro = fechaInicio?.ToString("yyyy-MM-dd");
            ViewBag.FechaFinFiltro = fechaFin?.ToString("yyyy-MM-dd");
            ViewBag.ResolucionInicioFiltro = resolucionInicio?.ToString("yyyy-MM-dd");
            ViewBag.ResolucionFinFiltro = resolucionFin?.ToString("yyyy-MM-dd");

            return View(historial);
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



        public async Task<IActionResult> DescargarInformeTecnico(string nombre, string prioridad, DateTime? fecha)
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

            var asignaciones = await query.OrderByDescending(x => x.FechaCreacion).ToListAsync();

            
            var pdfBytes = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Header().Text("Informe de Mis Asignaciones").FontSize(20).Bold();
                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm}");
                        col.Item().Text($"Total de asignaciones: {asignaciones.Count}").Bold();

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(50);   
                                columns.RelativeColumn(2);    
                                columns.RelativeColumn(2);    
                                columns.RelativeColumn(2);    
                                columns.RelativeColumn(1);    
                                columns.RelativeColumn(1);    
                                columns.RelativeColumn(1);    
                                columns.RelativeColumn(1);    
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("#").Bold();
                                header.Cell().Element(CellStyle).Text("Aplicación").Bold();
                                header.Cell().Element(CellStyle).Text("Categoría").Bold();
                                header.Cell().Element(CellStyle).Text("Prioridad").Bold();
                                header.Cell().Element(CellStyle).Text("Estado").Bold();
                                header.Cell().Element(CellStyle).Text("Creación").Bold();
                                header.Cell().Element(CellStyle).Text("Resolución").Bold();
                            });

                            foreach (var t in asignaciones)
                            {
                                table.Cell().Element(CellStyle).Text(t.TicketId.ToString());
                                table.Cell().Element(CellStyle).Text(t.NombreAplicacion);
                                table.Cell().Element(CellStyle).Text(t.Categoria);
                                table.Cell().Element(CellStyle).Text(t.UsuarioNombre);
                                table.Cell().Element(CellStyle).Text(t.Prioridad);
                                table.Cell().Element(CellStyle).Text(t.Estado);
                                table.Cell().Element(CellStyle).Text(t.FechaCreacion.ToString("dd/MM/yyyy"));
                                table.Cell().Element(CellStyle).Text(t.FechaResolucion != null ? ((DateTime)t.FechaResolucion).ToString("dd/MM/yyyy") : "Pendiente");
                            }
                        });
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

            return File(pdfBytes, "application/pdf", $"InformeMisAsignaciones_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

           
            static QuestPDF.Infrastructure.IContainer CellStyle(QuestPDF.Infrastructure.IContainer container)
            {
                return container.PaddingVertical(2).PaddingHorizontal(4);
            }
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
                    page.Margin(40);
                    page.Header().Text($"Informe de Ticket #{ticket.TicketId}").FontSize(20).Bold();

                    page.Content().Column(col =>
                    {
                        
                        col.Item().Text($"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm}");
                        col.Item().PaddingVertical(10).Text("Información del Ticket").Bold().FontSize(14);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1); 
                                columns.RelativeColumn(3); 
                            });

                            
                            table.Cell().Element(CellStyle).Text("Aplicación").Bold();
                            table.Cell().Element(CellStyle).Text(ticket.NombreAplicacion ?? "N/A");

                            table.Cell().Element(CellStyle).Text("Descripción").Bold();
                            table.Cell().Element(CellStyle).Text(ticket.Descripcion ?? "N/A");

                            table.Cell().Element(CellStyle).Text("Prioridad").Bold();
                            table.Cell().Element(CellStyle).Text(ticket.Prioridad ?? "N/A");

                            table.Cell().Element(CellStyle).Text("Estado").Bold();
                            table.Cell().Element(CellStyle).Text(ticket.Estado ?? "N/A");

                            table.Cell().Element(CellStyle).Text("Fecha de Creación").Bold();
                            table.Cell().Element(CellStyle).Text(ticket.FechaCreacion.ToString("dd/MM/yyyy"));

                            table.Cell().Element(CellStyle).Text("Fecha de Resolución").Bold();
                            table.Cell().Element(CellStyle).Text(ticket.FechaResolucion?.ToString("dd/MM/yyyy") ?? "Pendiente");

                            table.Cell().Element(CellStyle).Text("Usuario").Bold();
                            table.Cell().Element(CellStyle).Text($"{ticket.Usuario} ({ticket.UsuarioEmail})");

                            table.Cell().Element(CellStyle).Text("Categoría").Bold();
                            table.Cell().Element(CellStyle).Text(ticket.Categoria ?? "N/A");
                        });

                        
                        col.Item().PaddingTop(20).Text("Archivos Adjuntos").Bold().FontSize(14);
                        if (archivos.Any())
                        {
                            col.Item().Column(archivoCol =>
                            {
                                foreach (var archivo in archivos)
                                {
                                    archivoCol.Item().Text("- " + archivo);
                                }
                            });
                        }
                        else
                        {
                            col.Item().Text("No hay archivos adjuntos.");
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

            
            static QuestPDF.Infrastructure.IContainer CellStyle(QuestPDF.Infrastructure.IContainer container)
            {
                return container.Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5);
            }
        }



    }
}
