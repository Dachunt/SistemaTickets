using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaTickets.Models
{
    public class Usuarios
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo no válido")]
        [StringLength(100)]
        public string Email { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Phone(ErrorMessage = "Formato de teléfono inválido")]
        [StringLength(20)]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos  caracteres")]
        public string Contrasena { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un rol")]
        public int RolId { get; set; }

        [ForeignKey("RolId")]
        public Roles Rol { get; set; }

        [Display(Name = "¿Tiene empresa?")]
        public bool TieneEmpresa { get; set; }
        // Relación con UsuarioEmpresa
        public ICollection<UsuarioEmpresa> UsuarioEmpresa { get; set; } = new List<UsuarioEmpresa>();
    }
}
