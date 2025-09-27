// Models/BrokerDashboardViewModel.cs
using System.Collections.Generic;

namespace EC2_PROGRA1.Models
{
    public class BrokerDashboardViewModel
    {
        public List<Visita> VisitasDeHoy { get; set; }
        public List<Reserva> ReservasActivas { get; set; }
    }
}