using System.ComponentModel.DataAnnotations;

namespace SistemaTickets.Models
{
    public class Externo
    {
        [Key]
        public int ExternoId { get; set; }

        [Required]
        [MaxLength(100)]
        public string NombreEmpresa { get; set; }

        [Required]
        [MaxLength(100)]
        public string NombreResponsable { get; set; }

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [MaxLength(20)]
        public string Telefono { get; set; }
    }
}
