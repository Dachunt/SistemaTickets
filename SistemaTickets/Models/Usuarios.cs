using System.ComponentModel.DataAnnotations;

namespace SistemaTickets.Models
{
    public class Usuarios
    {
        [Key]
        public int UserId { get; set; }
        public string Nombre { get; set; } 
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Contrasena { get; set; }
        public int RolId { get; set; }
        public Boolean TieneEmpresa { get; set; }
    }
}
