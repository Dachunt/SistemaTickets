using SistemaTickets.Models;

namespace SistemaTickets.Models
{
    // Models/TicketAsignacionViewModel.cs
    public class TicketAsignacionViewModel
    {
        public int TicketId { get; set; }
        public string NombreAplicacion { get; set; }
        public string Descripcion { get; set; }
        public string Prioridad { get; set; }
        public string Estado { get; set; }
        public string Categoria { get; set; }

        public List<Usuarios>TecnicosDisponibles { get; set; }
    }



}
