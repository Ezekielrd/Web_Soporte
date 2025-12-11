namespace DGASoporte.Models
{
    public class DashboardKpiVM
    {
        public int TotalSolicitudesHistorico { get; set; }
        public int SolicitudesCerradas30Dias { get; set; }
        public int TareasAbiertasActuales { get; set; }
        public double PorcentajeSlaCumplido { get; set; }
    }
}
