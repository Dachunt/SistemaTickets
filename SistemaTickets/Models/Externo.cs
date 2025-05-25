using System.ComponentModel.DataAnnotations;

namespace SistemaTickets.Models
{
    public class Externo
    {
        [Key]
        public int ExternoId { get; set; }

        [Required(ErrorMessage = "El nombre de la empresa es obligatorio")]
        public string NombreEmpresa { get; set; }

        [Required(ErrorMessage = "El nombre del responsable es obligatorio")]
        public string NombreResponsable { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Phone]
        public string Telefono { get; set; }
    }
}
