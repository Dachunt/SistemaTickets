using System.ComponentModel.DataAnnotations;

namespace SistemaTickets.Models
{
    public class HistorialEstados
    {
        [Key]

        public int HistorialId { get; set; }

        public int TicketId { get; set; }

        [MaxLength(50)]
        public string EstadoAnterior { get; set; }

        [MaxLength(50)]
        public string EstadoNuevo { get; set; }

        public DateTime FechaCambio { get; set; } = DateTime.Now;

        public int CambiadoPor { get; set; }
    }
}
