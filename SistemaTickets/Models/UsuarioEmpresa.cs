using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaTickets.Models
{
    public class UsuarioEmpresa
    {
        [Key]
        public int UsuarioEmpresaId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int ExternoId { get; set; }

        [ForeignKey("UserId")]
        public Usuarios Usuario { get; set; }

        [ForeignKey("ExternoId")]
        public Externo Externo { get; set; }
    }
}
