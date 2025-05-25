using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaTickets.Models;

namespace SistemaTickets.Controllers
{
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
            // Obtener técnicos
            var tecnicos = await _context.Usuarios
                .Where(u => u.RolId == 2) // Técnicos
                .ToListAsync();

            // Obtener IDs de tickets que ya están asignados
            var ticketsAsignados = await _context.Asignaciones
                .Select(a => a.TicketId)
                .ToListAsync();

            // Obtener tickets abiertos que NO estén asignados
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
                TecnicosDisponibles = tecnicos
            }).ToList();

            return View(viewModels);
        }




        [HttpPost]
        public async Task<IActionResult> Asignar(int TicketId, int TecnicoId)
        {
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

            // Redirigir al listado para que se vea el cambio
            return RedirectToAction(nameof(Index));
        }




        // GET: Asignaciones/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asignaciones = await _context.Asignaciones
                .FirstOrDefaultAsync(m => m.AsignacionId == id);
            if (asignaciones == null)
            {
                return NotFound();
            }

            return View(asignaciones);
        }

        // GET: Asignaciones/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Asignaciones/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AsignacionId,TicketId,TecnicoId,FechaAsignacion,EstadoAsignacion")] Asignaciones asignaciones)
        {
            if (ModelState.IsValid)
            {
                _context.Add(asignaciones);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(asignaciones);
        }

        // GET: Asignaciones/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asignaciones = await _context.Asignaciones.FindAsync(id);
            if (asignaciones == null)
            {
                return NotFound();
            }
            return View(asignaciones);
        }

        // POST: Asignaciones/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AsignacionId,TicketId,TecnicoId,FechaAsignacion,EstadoAsignacion")] Asignaciones asignaciones)
        {
            if (id != asignaciones.AsignacionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(asignaciones);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AsignacionesExists(asignaciones.AsignacionId))
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
            return View(asignaciones);
        }

        // GET: Asignaciones/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asignaciones = await _context.Asignaciones
                .FirstOrDefaultAsync(m => m.AsignacionId == id);
            if (asignaciones == null)
            {
                return NotFound();
            }

            return View(asignaciones);
        }

        // POST: Asignaciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var asignaciones = await _context.Asignaciones.FindAsync(id);
            if (asignaciones != null)
            {
                _context.Asignaciones.Remove(asignaciones);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AsignacionesExists(int id)
        {
            return _context.Asignaciones.Any(e => e.AsignacionId == id);
        }
    }
}
