namespace DGASoporte.Models
{
    public class TecnicoRanking
    {
        public int TecnicoId { get; set; }
        public string NombreTecnico { get; set; } = string.Empty;
        public int TotalAtendidas { get; set; }
        public double TiempoPromedioResolucionHoras { get; set; }
        public double PorcentajeSlaCumplido { get; set; }
    }
}
