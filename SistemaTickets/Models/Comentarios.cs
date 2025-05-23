using System.ComponentModel.DataAnnotations;

namespace SistemaTickets.Models
{
    public class Comentarios
    {
        [Key]

        public int ComentarioId { get; set; }

        public int TicketId { get; set; }
        public int AutorId { get; set; }

        public string Comentario { get; set; }

        public DateTime FechaComentario { get; set; } = DateTime.Now;
    }
}
