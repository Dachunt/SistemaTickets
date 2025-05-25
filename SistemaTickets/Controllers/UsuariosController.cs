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

            // Validar correo duplicado
            bool correoExiste = await _context.Usuarios.AnyAsync(u => u.Email == usuario.Email);
            if (correoExiste)
            {
                TempData["Error"] = "El correo ingresado ya está registrado.";
                return View(usuario);
            }

            usuario.TieneEmpresa = usuario.Nombre == "Externo";

            // Hashear la contraseña
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
            return RedirectToAction("Registrar");
        }
        public async Task<IActionResult> Index()
        {
            var usuarios = await _context.Usuarios
                .Include(u => u.Rol)
                .Include(u => u.UsuarioEmpresa)
                .ToListAsync();

            ViewBag.Roles = await _context.Roles.ToListAsync();

            return View(usuarios);
        }

    }

}

