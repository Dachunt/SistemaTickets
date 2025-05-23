using System.ComponentModel.DataAnnotations;

namespace SistemaTickets.Models
{
    public class Categorias
    {
        [Key]
        public int CategoriaId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }
    }
}
