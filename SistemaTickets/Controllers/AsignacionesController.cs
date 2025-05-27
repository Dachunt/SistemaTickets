using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaTickets.Atributos;
using SistemaTickets.Models;

namespace SistemaTickets.Controllers
{
    [SesionActiva]
    public class AsignacionesController : Controller
    {
        private readonly SistemaTicketsContext _context;

        public AsignacionesController(SistemaTicketsContext context)
        {
            _context = context;
        }

        // GET: Asignaciones
        public async Task<IActionResult> Index()
        {
            // Obtener técnicos y administradores
            var tecnicosYAdmins = await _context.Usuarios
                .Where(u => u.RolId == 1 || u.RolId == 2) // Administradores y Técnicos
                .ToListAsync();

            // Obtener IDs de tickets que ya están asignados
            var ticketsAsignados = await _context.Asignaciones
                .Select(a => a.TicketId)
                .ToListAsync();

            // Obtener tickets abiertos que NO estén asignados, con su categoría
            var ticketsConCategoria = await (from t in _context.Tickets
                                             join c in _context.Categorias
                                             on t.CategoriaId equals c.CategoriaId
                                             where t.Estado == "Abierto"
                                             && !ticketsAsignados.Contains(t.TicketId)
                                             select new
                                             {
                                                 t.TicketId,
                                                 t.NombreAplicacion,
                                                 t.Descripcion,
                                                 t.Prioridad,
                                                 t.Estado,
                                                 CategoriaNombre = c.Nombre
                                             }).ToListAsync();

            // Crear la lista de ViewModels
            var viewModels = ticketsConCategoria.Select(t => new TicketAsignacionViewModel
            {
                TicketId = t.TicketId,
                NombreAplicacion = t.NombreAplicacion,
                Descripcion = t.Descripcion,
                Prioridad = t.Prioridad,
                Estado = t.Estado,
                Categoria = t.CategoriaNombre,
                TecnicosDisponibles = tecnicosYAdmins
            }).ToList();

            return View(viewModels);
        }





        [HttpPost]
        public async Task<IActionResult> Asignar(int TicketId, int TecnicoId)
        {
            // Verificar si el ticket ya está asignado
            bool yaAsignado = await _context.Asignaciones.AnyAsync(a => a.TicketId == TicketId);

            if (yaAsignado)
            {
                TempData["Error"] = "Este ticket ya está asignado a un técnico.";
                return RedirectToAction(nameof(Index));
            }

            // Crear la asignación
            var asignacion = new Asignaciones
            {
                TicketId = TicketId,
                TecnicoId = TecnicoId,
                FechaAsignacion = DateTime.Now,
                EstadoAsignacion = "Asignado"
            };

            _context.Asignaciones.Add(asignacion);
            await _context.SaveChangesAsync();

            var ticket = await _context.Tickets.FindAsync(TicketId);
            var tecnico = await _context.Usuarios.FindAsync(TecnicoId);

            if (ticket != null && tecnico != null)
            {
                var emailService = new EmailService();
                emailService.EnviarCorreoAsignacionTicket(
                    tecnico.Email,
                    tecnico.Nombre,
                    ticket.TicketId,
                    ticket.NombreAplicacion,
                    ticket.Descripcion,
                    asignacion.FechaAsignacion
                );
            }


            TempData["Success"] = "El ticket fue asignado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Details(int id)
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


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TicketId,Prioridad")] Tickets ticket)
        {
            if (id != ticket.TicketId)
            {
                return NotFound();
            }
            try
            {
                var ticketExistente = await _context.Tickets.FindAsync(id);
                if (ticketExistente == null)
                {
                    return NotFound();
                }

                ticketExistente.Prioridad = ticket.Prioridad;
                _context.Update(ticketExistente);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Tickets.Any(e => e.TicketId == ticket.TicketId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

        }

        private bool AsignacionesExists(int id)
        {
            return _context.Asignaciones.Any(e => e.AsignacionId == id);
        }
        public async Task<IActionResult> History()
        {
            var usuarios = await _context.Usuarios
                .Where(u => u.RolId == 3) 
                .ToListAsync();

            // Obtener tickets abiertos que NO estén asignados, con su categoría
            var ticketsConCategoria = await (from t in _context.Tickets
                                             join c in _context.Categorias
                                             on t.CategoriaId equals c.CategoriaId
                                             where t.Estado == "Abierto"
                                             select new
                                             {
                                                 t.TicketId,
                                                 t.NombreAplicacion,
                                                 t.Descripcion,
                                                 t.Prioridad,
                                                 t.Estado,
                                                 CategoriaNombre = c.Nombre
                                             }).ToListAsync();

            // Crear la lista de ViewModels
            var viewModels = ticketsConCategoria.Select(t => new TicketAsignacionViewModel
            {
                TicketId = t.TicketId,
                NombreAplicacion = t.NombreAplicacion,
                Descripcion = t.Descripcion,
                Prioridad = t.Prioridad,
                Estado = t.Estado,
                Categoria = t.CategoriaNombre
            }).ToList();

            return View(viewModels);
        }

    }
}
