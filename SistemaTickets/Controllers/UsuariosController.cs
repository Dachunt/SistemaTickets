using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaTickets.Models;

namespace SistemaTickets.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly SistemaTicketsContext _context;

        public UsuariosController(SistemaTicketsContext context)
        {
            _context = context;
        }

        //// GET: Usuarios
        //public async Task<IActionResult> Index()
        //{
        //    // Incluir rol para mostrar nombre en la vista
        //    var usuarios = await _context.Usuarios.Include(u => u.RolId).ToListAsync();
        //    return View(usuarios);
        //}

        //// GET: Usuarios/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null)
        //        return NotFound();

        //    var usuario = await _context.Usuarios
        //        .Include(u => u.RolId)
        //        .FirstOrDefaultAsync(m => m.UserId == id);

        //    if (usuario == null)
        //        return NotFound();

        //    return View(usuario);
        //}

        //// GET: Usuarios/Create
        //[HttpGet]
        //public async Task<IActionResult> Create()
        //{
        //    ViewBag.Roles = await _context.Roles.ToListAsync();
        //    return View();
        //}

        //// POST: Usuarios/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(Usuarios usuario)
        //{
        //    ViewBag.Roles = await _context.Roles.ToListAsync();

        //    if (string.IsNullOrWhiteSpace(usuario.Nombre))
        //        ModelState.AddModelError(nameof(usuario.Nombre), "El nombre es obligatorio.");


        //    if (string.IsNullOrWhiteSpace(usuario.Email))
        //        ModelState.AddModelError(nameof(usuario.Email), "El correo electrónico es obligatorio.");

        //    if (string.IsNullOrWhiteSpace(usuario.Contrasena))
        //        ModelState.AddModelError(nameof(usuario.Contrasena), "La contraseña es obligatoria.");

        //    if (usuario.RolId <= 0)
        //        ModelState.AddModelError(nameof(usuario.RolId), "Debe seleccionar un rol válido.");

        //    // Validar formato email
        //    if (!string.IsNullOrWhiteSpace(usuario.Email))
        //    {
        //        try
        //        {
        //            var emailCheck = new MailAddress(usuario.Email);
        //        }
        //        catch
        //        {
        //            ModelState.AddModelError(nameof(usuario.Email), "El correo electrónico no tiene un formato válido.");
        //        }
        //    }

        //    // Validar contraseña segura
        //    if (!string.IsNullOrWhiteSpace(usuario.Contrasena))
        //    {
        //        var passwordRegex = new Regex(@"^(?=.*[A-Z])(?=.*\d).{6,}$");
        //        if (!passwordRegex.IsMatch(usuario.Contrasena))
        //            ModelState.AddModelError(nameof(usuario.Contrasena), "La contraseña debe tener al menos 6 caracteres, incluir una mayúscula y un número.");
        //    }

        //    // Validar email único
        //    if (!string.IsNullOrWhiteSpace(usuario.Email))
        //    {
        //        bool emailExists = await _context.Usuarios.AnyAsync(u => u.Email == usuario.Email);
        //        if (emailExists)
        //            ModelState.AddModelError(nameof(usuario.Email), "Ya existe un usuario registrado con ese correo.");
        //    }

        //    // Validar campos empresa si es usuario externo
        //    if (usuario.TieneEmpresa)
        //    {
        //        if (string.IsNullOrWhiteSpace(usuario.NombreEmpresa))
        //            ModelState.AddModelError(nameof(usuario.NombreEmpresa), "El nombre de la empresa es obligatorio para usuarios externos.");

        //        if (string.IsNullOrWhiteSpace(usuario.ContactoEmpresa))
        //            ModelState.AddModelError(nameof(usuario.ContactoEmpresa), "El contacto principal es obligatorio para usuarios externos.");

        //        if (string.IsNullOrWhiteSpace(usuario.TelefonoEmpresa))
        //            ModelState.AddModelError(nameof(usuario.TelefonoEmpresa), "El teléfono de la empresa es obligatorio para usuarios externos.");
        //    }

        //    if (!ModelState.IsValid)
        //        return View(usuario);
            
        //    //NUEVO USUARIO
        //    var nuevoUsuario = new Usuarios
        //    {
        //        Nombre = usuario.Nombre.Trim(),
        //        Email = usuario.Email.Trim(),
        //        Contrasena = HashPassword(usuario.Contrasena),
        //        RolId = usuario.RolId,
        //        TieneEmpresa = usuario.TieneEmpresa,
        //        NombreEmpresa = usuario.TieneEmpresa ? usuario.NombreEmpresa.Trim() : null,
        //        ContactoEmpresa = usuario.TieneEmpresa ? usuario.ContactoEmpresa.Trim() : null,
        //        TelefonoEmpresa = usuario.TieneEmpresa ? usuario.TelefonoEmpresa.Trim() : null
        //    };

        //    _context.Usuarios.Add(nuevoUsuario);
        //    await _context.SaveChangesAsync();
            
        //    //CORREO
        //    TempData["Mensaje"] = "Usuario creado correctamente.";
        //    return RedirectToAction(nameof(Index));
        //}

        //// GET: Usuarios/Edit/5
        //[HttpGet]
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //        return NotFound();

        //    var usuario = await _context.Usuarios.FindAsync(id);
        //    if (usuario == null)
        //        return NotFound();

        //    ViewBag.Roles = await _context.Roles.ToListAsync();
        //    return View(usuario);
        //}

        //// POST: Usuarios/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, Usuarios usuario)
        //{
        //    if (id != usuario.UserId)
        //        return NotFound();

        //    ViewBag.Roles = await _context.Roles.ToListAsync();

        //    if (string.IsNullOrWhiteSpace(usuario.Nombre))
        //        ModelState.AddModelError(nameof(usuario.Nombre), "El nombre es obligatorio.");


        //    if (string.IsNullOrWhiteSpace(usuario.Email))
        //        ModelState.AddModelError(nameof(usuario.Email), "El correo electrónico es obligatorio.");

        //    if (usuario.RolId <= 0)
        //        ModelState.AddModelError(nameof(usuario.RolId), "Debe seleccionar un rol válido.");

        //    // Validar email formato
        //    if (!string.IsNullOrWhiteSpace(usuario.Email))
        //    {
        //        try
        //        {
        //            var emailCheck = new MailAddress(usuario.Email);
        //        }
        //        catch
        //        {
        //            ModelState.AddModelError(nameof(usuario.Email), "El correo electrónico no tiene un formato válido.");
        //        }
        //    }

        //    // Validar email único (excepto este usuario)
        //    if (!string.IsNullOrWhiteSpace(usuario.Email))
        //    {
        //        bool emailExists = await _context.Usuarios
        //            .AnyAsync(u => u.Email == usuario.Email && u.UserId != usuario.UserId);
        //        if (emailExists)
        //            ModelState.AddModelError(nameof(usuario.Email), "Ya existe otro usuario registrado con ese correo.");
        //    }

        //    // Validar campos empresa si es usuario externo
        //    if (usuario.TieneEmpresa)
        //    {
        //        if (string.IsNullOrWhiteSpace(usuario.NombreEmpresa))
        //            ModelState.AddModelError(nameof(usuario.NombreEmpresa), "El nombre de la empresa es obligatorio para usuarios externos.");

        //        if (string.IsNullOrWhiteSpace(usuario.ContactoEmpresa))
        //            ModelState.AddModelError(nameof(usuario.ContactoEmpresa), "El contacto principal es obligatorio para usuarios externos.");

        //        if (string.IsNullOrWhiteSpace(usuario.TelefonoEmpresa))
        //            ModelState.AddModelError(nameof(usuario.TelefonoEmpresa), "El teléfono de la empresa es obligatorio para usuarios externos.");
        //    }

        //    if (!ModelState.IsValid)
        //        return View(usuario);

        //    try
        //    {
        //        var usuarioExistente = await _context.Usuarios.FindAsync(id);
        //        if (usuarioExistente == null)
        //            return NotFound();

        //        usuarioExistente.Nombre = usuario.Nombre.Trim();
        //        usuarioExistente.Email = usuario.Email.Trim();
        //        usuarioExistente.RolId = usuario.RolId;
        //        usuarioExistente.TieneEmpresa = usuario.TieneEmpresa;
        //        usuarioExistente.NombreEmpresa = usuario.TieneEmpresa ? usuario.NombreEmpresa.Trim() : null;
        //        usuarioExistente.ContactoEmpresa = usuario.TieneEmpresa ? usuario.ContactoEmpresa.Trim() : null;
        //        usuarioExistente.TelefonoEmpresa = usuario.TieneEmpresa ? usuario.TelefonoEmpresa.Trim() : null;

        //        // Actualizar contraseña solo si viene no vacía y cumple condiciones
        //        if (!string.IsNullOrWhiteSpace(usuario.Contrasena))
        //        {
        //            var passwordRegex = new Regex(@"^(?=.*[A-Z])(?=.*\d).{6,}$");
        //            if (!passwordRegex.IsMatch(usuario.Contrasena))
        //            {
        //                ModelState.AddModelError(nameof(usuario.Contrasena), "La contraseña debe tener al menos 6 caracteres, incluir una mayúscula y un número.");
        //                return View(usuario);
        //            }
        //            usuarioExistente.Contrasena = HashPassword(usuario.Contrasena);
        //        }

        //        _context.Update(usuarioExistente);
        //        await _context.SaveChangesAsync();

        //        TempData["Mensaje"] = "Usuario actualizado correctamente.";
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!UsuarioExists(id))
        //            return NotFound();
        //        else
        //            throw;
        //    }
        //}

        //// GET: Usuarios/Delete/5
        //[HttpGet]
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //        return NotFound();

        //    var usuario = await _context.Usuarios
        //        .Include(u => u.RolId)
        //        .FirstOrDefaultAsync(m => m.UserId == id);

        //    if (usuario == null)
        //        return NotFound();

        //    return View(usuario);
        //}

        //// POST: Usuarios/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var usuario = await _context.Usuarios.FindAsync(id);
        //    if (usuario != null)
        //    {
        //        _context.Usuarios.Remove(usuario);
        //        await _context.SaveChangesAsync();
        //        TempData["Mensaje"] = "Usuario eliminado correctamente.";
        //    }
        //    else
        //    {
        //        TempData["Error"] = "No se encontró el usuario para eliminar.";
        //    }
        //    return RedirectToAction(nameof(Index));
        //}

        //private bool UsuarioExists(int id)
        //{
        //    return _context.Usuarios.Any(e => e.UserId == id);
        //}

        //private string HashPassword(string password)
        //{
        //    // Ejemplo simple para demo. En producción usar Identity o librería especializada.
        //    return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
        //}
    }
}
