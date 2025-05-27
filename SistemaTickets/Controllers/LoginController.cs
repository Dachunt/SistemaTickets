using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SistemaTickets.Models;

namespace SistemaTickets.Controllers
{
    [SesionActiva]

    public class LoginController : Controller
    {
        private readonly SistemaTicketsContext _sistemaContext;
        private readonly PasswordHasher<Usuarios> _hasher;


        public LoginController(SistemaTicketsContext context)
        {
            _sistemaContext = context;
            _hasher = new PasswordHasher<Usuarios>();

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
            //var user = _sistemaContext.Usuarios
            //    .FirstOrDefault(u => u.Email == email && u.Contrasena == contrasena);
            var user = _sistemaContext.Usuarios
               .FirstOrDefault(u => u.Email == email);

            var resultado = _hasher.VerifyHashedPassword(user, user.Contrasena, contrasena);

            if (resultado != PasswordVerificationResult.Success)
            {
                user = null; // Si la verificación falla, establecemos user como null
            }

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

    }

}
