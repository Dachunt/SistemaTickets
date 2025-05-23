using System.ComponentModel.DataAnnotations;

namespace SistemaTickets.Models
{
    public class UsuarioEmpresa
    {
        [Key]

        public int UsuarioEmpresaId { get; set; }

        public int UserId { get; set; }
        public int ExternoId { get; set; }
    }
}
