using System.ComponentModel.DataAnnotations;

namespace SistemaTickets.Models
{
    public class Tickets
    {
        [Key]

        public int TicketId { get; set; }
        public int UserId { get; set; }
        public int CategoriaId { get; set; }

        [MaxLength(100)]
        public string NombreAplicacion { get; set; }

        public string Descripcion { get; set; }

        [MaxLength(20)]
        public string Prioridad { get; set; }

        [MaxLength(50)]
        public string Estado { get; set; } = "Abierto";

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}
