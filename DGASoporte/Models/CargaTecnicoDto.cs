namespace DGASoporte.Models
{
    public class CargaTecnicoDto
    {
        public int? TecnicoId { get; set; }
        public string NombreTecnico { get; set; } = "Sin técnico";
        public int TareasAbiertas { get; set; }
        public int TareasCerradas30Dias { get; set; }
    }
}
