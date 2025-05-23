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
    }
}
