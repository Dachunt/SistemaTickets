using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaTickets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SistemaTickets.Controllers
{
    public class TicketsController : Controller
    {
        private readonly SistemaTicketsContext _context;

        public TicketsController(SistemaTicketsContext context)
        {
            _context = context;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            ViewBag.MensajeExito = TempData["MensajeExito"];
            return View(await _context.Tickets.ToListAsync());
        }

       


        // GET: Tickets/Create
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("id_usuario");
            if (userId == null) return RedirectToAction("Login", "Login");

            var usuario = _context.Usuarios.FirstOrDefault(u => u.UserId == userId);
            if (usuario == null) return RedirectToAction("Login", "Login");

            ViewBag.Nombre = usuario.Nombre;
            ViewBag.Telefono = usuario.Telefono;
            ViewBag.Email = usuario.Email;

            ViewBag.Categorias = new SelectList(_context.Categorias.ToList(), "CategoriaId", "Nombre");
            return View();
        }

        // POST: Tickets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tickets tickets, List<IFormFile> Archivos)
        {
            var userId = HttpContext.Session.GetInt32("id_usuario");
            var userRol = HttpContext.Session.GetInt32("rol");

            var NombreRol = _context.Roles.FirstOrDefault(r => r.RolId == userRol);
            if( NombreRol == null)
            {
                return RedirectToAction("Login", "Login");

            }
            if (userId == null) return RedirectToAction("Login", "Login");

            if (ModelState.IsValid)
            {
                tickets.UserId = userId.Value;
                tickets.FechaCreacion = DateTime.Now;
                tickets.Estado = "Abierto";

                _context.Add(tickets);
                await _context.SaveChangesAsync(); 

                if (Archivos != null && Archivos.Count > 0)
                {
                    var carpetaDestino = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ArchivosTickets");
                    if (!Directory.Exists(carpetaDestino)) Directory.CreateDirectory(carpetaDestino);

                    foreach (var archivo in Archivos)
                    {
                        if (archivo.Length > 0)
                        {
                            var nombreOriginal = Path.GetFileName(archivo.FileName);
                            var nombreUnico = $"{Guid.NewGuid()}_{nombreOriginal}";
                            var rutaCompleta = Path.Combine(carpetaDestino, nombreUnico);

                            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                            {
                                await archivo.CopyToAsync(stream);
                            }

                            _context.ArchivosAdjuntos.Add(new ArchivosAdjuntos
                            {
                                TicketId = tickets.TicketId,
                                NombreArchivo = nombreOriginal,
                                RutaArchivo = $"/ArchivosTickets/{nombreUnico}",
                                FechaSubida = DateTime.Now
                            });
                        }
                    }

                    await _context.SaveChangesAsync();

                }
                
                if (NombreRol.NombreRol == "Administrador")
                {
                    TempData["MensajeExitoAdmin"] = "El ticket fue creado exitosamente.";
                    return RedirectToAction("Index", "Home");
                }
                else if (NombreRol.NombreRol == "Tecnico")
                {
                    TempData["MensajeExito"] = "El ticket fue creado exitosamente.";
                    return RedirectToAction("Home", "SoporteTecnico");
                }
                else if(NombreRol.NombreRol == "Externo")
                {
                    TempData["MensajeExitoExterno"] = "El ticket fue creado exitosamente.";
                    return RedirectToAction("HomeExterno", "Usuarios");
                }
               // return RedirectToAction("Home", "SoporteTecnico");
            }

            var usuario = _context.Usuarios.FirstOrDefault(u => u.UserId == userId);
            ViewBag.Nombre = usuario?.Nombre;
            ViewBag.Telefono = usuario?.Telefono;
            ViewBag.Email = usuario?.Email;
            ViewBag.Categorias = new SelectList(_context.Categorias.ToList(), "CategoriaId", "Nombre", tickets.CategoriaId);

            return View(tickets);
        }


        [HttpPost]
        public IActionResult EliminarArchivo(int archivoId)
        {
            var archivo = _context.ArchivosAdjuntos.Find(archivoId);
            if (archivo != null)
            {
                _context.ArchivosAdjuntos.Remove(archivo);
                _context.SaveChanges();
            }

            return RedirectToAction("Edit", new { id = archivo.TicketId }); // o al listado
        }


        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tickets = await _context.Tickets.FindAsync(id);
            if (tickets == null)
            {
                return NotFound();
            }
            return View(tickets);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TicketId,UserId,CategoriaId,NombreAplicacion,Descripcion,Prioridad,Estado,FechaCreacion")] Tickets tickets)
        {
            if (id != tickets.TicketId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tickets);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketsExists(tickets.TicketId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(tickets);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tickets = await _context.Tickets
                .FirstOrDefaultAsync(m => m.TicketId == id);
            if (tickets == null)
            {
                return NotFound();
            }

            return View(tickets);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tickets = await _context.Tickets.FindAsync(id);
            if (tickets != null)
            {
                _context.Tickets.Remove(tickets);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketsExists(int id)
        {
            return _context.Tickets.Any(e => e.TicketId == id);
        }

          public async Task<IActionResult> TicketsAsignados(string nombreTecnico, DateTime? fechaInicio, DateTime? fechaFin)
          {
              var query = _context.Asignaciones
                  .Include(a => a.Ticket)
                  .Include(a => a.Tecnico)
                  .Where(a => a.EstadoAsignacion == "Asignado" && (a.Tecnico.RolId == 1 || a.Tecnico.RolId == 2))
                  .AsQueryable();

              if (!string.IsNullOrEmpty(nombreTecnico))
              {
                  query = query.Where(a => a.Tecnico.Nombre.Contains(nombreTecnico));
              }

              if (fechaInicio.HasValue)
              {
                  query = query.Where(a => a.Ticket.FechaCreacion >= fechaInicio.Value);
              }

              if (fechaFin.HasValue)
              {
                  query = query.Where(a => a.Ticket.FechaCreacion <= fechaFin.Value);
              }

              var asignaciones = await query.ToListAsync();

              return View(asignaciones);
          }


       

    }
}
