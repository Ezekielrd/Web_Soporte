namespace DGASoporte.Models
{
    public class DashboardVM
    {
        // Cards superiores
        public DashboardKpiVM Kpis { get; set; } = new DashboardKpiVM();

        // Gráfico 1: tendencias por mes
        public List<SerieMesDto> SolicitudesPorMes { get; set; } = new();
        public List<SerieMesDto> SolicitudesCerradasPorMes { get; set; } = new();

        // Gráfico 2: carga por técnico
        public List<CargaTecnicoDto> CargaPorTecnico { get; set; } = new();

        // Gráfico 3: distribución por estado de solicitud
        public List<EstadoDistribucionDto> DistribucionPorEstado { get; set; } = new();

        // Gráfico 4: top unidades
        public List<UnidadTopDto> TopUnidadesPorSolicitudes { get; set; } = new();
    }
}
