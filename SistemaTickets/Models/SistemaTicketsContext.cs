using Microsoft.EntityFrameworkCore;

namespace SistemaTickets.Models
{
    public class SistemaTicketsContext : DbContext
    {
        public SistemaTicketsContext(DbContextOptions<SistemaTicketsContext> options): base(options)
        {
        }

        public DbSet<Tickets> Tickets { get; set; } 
        public DbSet<Usuarios> Usuarios { get; set; }
        public DbSet<UsuarioEmpresa> UsuarioEmpresa { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<HistorialEstados> HistorialEstados { get; set; }
        public DbSet<Externo> Externo { get; set; }
        public DbSet<Comentarios> Comentarios { get; set; }
        public DbSet<Categorias> Categorias { get; set; }
        public DbSet<Asignaciones> Asignaciones { get; set; }
        public DbSet<ArchivosAdjuntos> ArchivosAdjuntos { get; set; }

    }
}

