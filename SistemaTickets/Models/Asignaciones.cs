using System.ComponentModel.DataAnnotations;

namespace SistemaTickets.Models
{
    public class Asignaciones
    {
        [Key]

        public int AsignacionId { get; set; }

        public int TicketId { get; set; }
        public int TecnicoId { get; set; }

        public DateTime FechaAsignacion { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string EstadoAsignacion { get; set; } = "Asignado";

        public virtual Tickets Ticket { get; set; }
        public virtual Usuarios Tecnico { get; set; } // Suponiendo que el técnico es un usuario

    }
}
