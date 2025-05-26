using System;

namespace SistemaTickets.Models
{
    public class TicketsPersonalizadoViewModel
    {
        public int TicketId { get; set; }
        public string UsuarioNombre { get; set; }
        public string CategoriaNombre { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string Estado { get; set; }
    }
}
