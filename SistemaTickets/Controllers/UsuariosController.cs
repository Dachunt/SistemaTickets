using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaTickets.Models;

namespace SistemaTickets.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly SistemaTicketsContext _context;
        private readonly PasswordHasher<Usuarios> _hasher;

        public UsuariosController(SistemaTicketsContext context)
        {
            _context = context;
            _hasher = new PasswordHasher<Usuarios>();
        }
        [HttpGet]
        public IActionResult Registrar()
        {
            var roles = _context.Roles.ToList();
            ViewBag.Roles = roles;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(Usuarios usuario, string NombreEmpresa, string NombreResponsable, string EmailEmpresa, string TelefonoEmpresa)
        {
            ViewBag.Roles = _context.Roles.ToList();

            bool correoExiste = await _context.Usuarios.AnyAsync(u => u.Email == usuario.Email);
            if (correoExiste)
            {
                TempData["Error"] = "El correo ingresado ya se encuentra registrado.";
                return View(usuario);
            }

            bool correoEmpresaExiste = await _context.Externo.AnyAsync(e => e.Email == EmailEmpresa);
            if (correoEmpresaExiste)
            {
                TempData["Error"] = "El correo de la empresa ya se encuentra registrado.";
                return View(usuario);
            }


            var rol = await _context.Roles.FindAsync(usuario.RolId);
            usuario.TieneEmpresa = rol != null && rol.NombreRol == "Externo";

            usuario.Contrasena = _hasher.HashPassword(usuario, usuario.Contrasena);

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            if (usuario.TieneEmpresa)
            {
                var externo = new Externo
                {
                    NombreEmpresa = NombreEmpresa,
                    NombreResponsable = NombreResponsable,
                    Email = EmailEmpresa,
                    Telefono = TelefonoEmpresa
                };

                _context.Externo.Add(externo);
                await _context.SaveChangesAsync();

                var relacion = new UsuarioEmpresa
                {
                    UserId = usuario.UserId,
                    ExternoId = externo.ExternoId
                };

                _context.UsuarioEmpresa.Add(relacion);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Usuario registrado correctamente.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index()
        {
            var usuarios = await _context.Usuarios
                .Include(u => u.Rol)
                .Include(u => u.UsuarioEmpresa)
                .Where(u => u.Estado == true)
                .ToListAsync();

            ViewBag.Roles = await _context.Roles.ToListAsync();

            return View(usuarios);
        }
        public async Task<IActionResult> VerEmpresas(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.UsuarioEmpresa)
                    .ThenInclude(ue => ue.Externo)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (usuario == null)
                return NotFound();

            ViewBag.UsuarioId = id;
            return View(usuario);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarEmpresa(int userId, string NombreEmpresa, string NombreResponsable, string EmailEmpresa, string TelefonoEmpresa)
        {
            var usuario = await _context.Usuarios.FindAsync(userId);

            if (usuario == null)
                return NotFound();

            bool correoEmpresaExiste = await _context.Externo.AnyAsync(e => e.Email == EmailEmpresa);
            if (correoEmpresaExiste)
            {
                TempData["Error"] = "El correo de la empresa ya se encuentra registrado.";
                return RedirectToAction("VerEmpresas", new { id = userId });

            }

            var nuevaEmpresa = new Externo
            {
                NombreEmpresa = NombreEmpresa,
                NombreResponsable = NombreResponsable,
                Email = EmailEmpresa,
                Telefono = TelefonoEmpresa
            };

            _context.Externo.Add(nuevaEmpresa);
            await _context.SaveChangesAsync();

            var relacion = new UsuarioEmpresa
            {
                UserId = userId,
                ExternoId = nuevaEmpresa.ExternoId
            };

            _context.UsuarioEmpresa.Add(relacion);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Empresa agregada exitosamente.";
            return RedirectToAction("VerEmpresas", new { id = userId });
        }
        // GET: Usuarios/Detalles/5
        [HttpGet]
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .Include(u => u.UsuarioEmpresa)
                    .ThenInclude(ue => ue.Externo)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (usuario == null)
            {
                return NotFound();
            }

            ViewBag.UsuarioId = usuario.UserId;
            return View(usuario);
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.UserId == id);
            if (usuario == null)
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToAction("Index");
            }

            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(Usuarios usuario)
        {
            var usuarioExistente = await _context.Usuarios.FirstOrDefaultAsync(u => u.UserId == usuario.UserId);

            if (usuarioExistente == null)
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToAction("Index");
            }

            usuarioExistente.Nombre = usuario.Nombre;
            usuarioExistente.Email = usuario.Email;
            usuarioExistente.Telefono = usuario.Telefono;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Usuario actualizado correctamente.";
            return RedirectToAction("Index");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Desactivar(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.UsuarioEmpresa)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (usuario == null)
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToAction("Index");
            }

            var tieneTicket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.UserId == id);
            if (tieneTicket != null)
            {
                TempData["Error"] = "El usuario tiene tickets asociados. No se puede desactivar.";
                return RedirectToAction("Index");
            }

            if (usuario.UsuarioEmpresa == null || usuario.UsuarioEmpresa.Count == 0)
            {
                usuario.Estado = false;
                _context.Usuarios.Update(usuario);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Usuario desactivado correctamente.";
            }
            else
            {
                TempData["Error"] = "El usuario tiene empresas asociadas. No se puede desactivar.";
            }

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Perfil()
        {
            var userId = HttpContext.Session.GetInt32("id_usuario");
            if (userId == null)
            {
                return RedirectToAction("Login", "Login");
            }
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (usuario == null)
            {
                return NotFound();
            }
            return View(usuario);

        }
        public  async Task<IActionResult> CambiarContra(string contraActual)
        {

            return View();
        }
        //Parte de dashboard de usuarios externos 

        public IActionResult HomeExterno()
        {
            ViewBag.MensajeExito = TempData["MensajeExitoExterno"] as string;
            return View();
        }


    }

}

