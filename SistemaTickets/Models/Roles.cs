using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;

namespace SistemaTickets.Models
{
    public class Roles
    {
        [Key] 
        public int RolId { get; set; }
        public string NombreRol { get; set; }
    }
}
