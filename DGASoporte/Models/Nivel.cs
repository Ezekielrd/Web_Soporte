namespace DGASoporte.Models
{
    public class Nivel
    {
         public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public ICollection<Tecnico> Tecnicos { get; set; } = new List<Tecnico>();

    }
}
