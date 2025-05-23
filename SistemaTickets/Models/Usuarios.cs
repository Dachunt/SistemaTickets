using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaTickets.Models
{
    public class Usuarios
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(100)]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido")]
        [StringLength(100)]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "La contraseña debe tener al menos 8 caracteres, incluir una letra, un número y un carácter especial.")]
        public string Contrasena { get; set; }

        [Required(ErrorMessage = "Seleccione un rol válido")]
        public int RolId { get; set; }

        public bool TieneEmpresa { get; set; } = false;

        // Campos para usuarios externos
        [StringLength(200)]
        public string NombreEmpresa { get; set; }

        [StringLength(100)]
        public string ContactoEmpresa { get; set; }

        [Phone(ErrorMessage = "Ingrese un número de teléfono válido")]
        [StringLength(20)]
        public string TelefonoEmpresa { get; set; }
    }
}
