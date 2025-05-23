using Microsoft.AspNetCore.Mvc;

namespace SistemaTickets.Controllers
{
    public class SoporteTecnicoController : Controller
    {
        public IActionResult Home()
        {
            return View();
        }
    }
}
