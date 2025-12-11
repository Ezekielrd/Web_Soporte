namespace DGASoporte.Models
{
    public class SerieMesDto
    {
        public int Anio { get; set; }
        public int Mes { get; set; }
        public int Cantidad { get; set; }

        public string EtiquetaMes =>
            new DateTime(Anio, Mes, 1).ToString("MMM yyyy");
    }
}
