using System.ComponentModel.DataAnnotations;

namespace SistemaTickets.Models
{
    public class ArchivosAdjuntos
    {
        [Key]

        public int ArchivoId { get; set; }

        public int TicketId { get; set; }

        [MaxLength(255)]
        public string NombreArchivo { get; set; }

        [MaxLength(500)]
        public string RutaArchivo { get; set; }

        public DateTime FechaSubida { get; set; } = DateTime.Now;
    }
}
