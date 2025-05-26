using Microsoft.AspNetCore.Mvc;
using SistemaTickets.Models;

namespace SistemaTickets.Controllers
{
    public class LoginController : Controller
    {
        private readonly SistemaTicketsContext _sistemaContext;

        public LoginController(SistemaTicketsContext context)
        {
            _sistemaContext = context;
        }

        // GET: Registro
        public IActionResult Registro()
        {
            return View();
        }

        // POST: Registro
        [HttpPost]
        public IActionResult Registro(Usuarios usuario)
        {
            if (ModelState.IsValid)
            {
                // Verificar si ya existe un usuario con ese correo
                var existe = _sistemaContext.Usuarios.FirstOrDefault(u => u.Email == usuario.Email && u.Estado == true);
                if (existe != null)
                {
                    ViewBag.Error = "El correo ya está registrado.";
                    return View(usuario);
                }

                _sistemaContext.Usuarios.Add(usuario);
                _sistemaContext.SaveChanges();

                // Guardamos la sesión con los datos del usuario
                HttpContext.Session.SetInt32("id_usuario", usuario.UserId);
                HttpContext.Session.SetString("nombre", usuario.Nombre);
                HttpContext.Session.SetInt32("rol", usuario.RolId);

                return RedirectToAction("Login", "Login");
            }

            return View(usuario);
        }

        // GET: Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        public IActionResult Login(string email, string contrasena)
        {
            var user = _sistemaContext.Usuarios
                .FirstOrDefault(u => u.Email == email && u.Contrasena == contrasena);

            if (user != null)
            {

                HttpContext.Session.SetInt32("id_usuario", user.UserId);
                HttpContext.Session.SetString("nombre", user.Nombre);
                HttpContext.Session.SetInt32("rol", user.RolId);
                var rol = _sistemaContext.Roles.FirstOrDefault(r => r.RolId == user.RolId);
                if (rol == null)
                {
                    return View();
                }
                HttpContext.Session.SetString("rol_nombre", rol.NombreRol);
                switch (user.RolId)
                {
                    case 1:
                        return RedirectToAction("Index", "Home");
                    case 2:
                        return RedirectToAction("Home", "SoporteTecnico");
                    default:
                        return RedirectToAction("HomeExterno", "Usuarios");
                }
                
            }

            ViewBag.Error = "Correo o contraseña incorrectos.";
            return View();

        }

        // Cerrar sesión
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // GET: Index
        public IActionResult Index()
        {
            return View();
        }
    }

}
