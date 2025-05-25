using SistemaTickets.Models;



namespace SistemaTickets.Models
{
    public class DashboardViewModel
    {
        
            public int TotalAbiertos { get; set; }
            public int TotalEnProgreso { get; set; }
            public int TotalResueltos { get; set; }

            public Dictionary<string, int> TicketsPorCategoria { get; set; }
            public Dictionary<string, int> TicketsPorUsuario { get; set; }
            public Dictionary<string, int> TicketsPorDiaSemana { get; set; }

            public Dictionary<int, double> TiemposResolucionHoras { get; set; }

        public double PromedioTicketsDiarios { get; set; }
            public DateTime FechaInicio { get; set; }
            public DateTime FechaFin { get; set; }

      


    }
}
