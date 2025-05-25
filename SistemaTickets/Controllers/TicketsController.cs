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
            return View(await _context.Tickets.ToListAsync());
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
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

        // GET: Tickets/Create
        //public IActionResult Create()
        //{
        //    ViewData["CategoriaId"] = new SelectList(_context.Categorias, "CategoriaId", "Nombre");
        //    ViewData["UserId"] = new SelectList(_context.Usuarios, "UserId", "Nombre");
        //    return View();
        //}


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
            if (userId == null) return RedirectToAction("Login", "Login");

            if (ModelState.IsValid)
            {
                tickets.UserId = userId.Value;
                tickets.FechaCreacion = DateTime.Now;
                tickets.Estado = "Abierto";

                _context.Add(tickets);
                await _context.SaveChangesAsync(); // Genera el TicketId

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

                return RedirectToAction("Index", "Home");
            }

            var usuario = _context.Usuarios.FirstOrDefault(u => u.UserId == userId);
            ViewBag.Nombre = usuario?.Nombre;
            ViewBag.Telefono = usuario?.Telefono;
            ViewBag.Email = usuario?.Email;
            ViewBag.Categorias = new SelectList(_context.Categorias.ToList(), "CategoriaId", "Nombre", tickets.CategoriaId);

            return View(tickets);
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
    }
}
