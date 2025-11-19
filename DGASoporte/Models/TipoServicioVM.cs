namespace DGASoporte.Models
{
    public class TipoServicioVM
    {
        public int Id { get; set; }
        public string Nombre { get; set; } =string.Empty;
        public int CategoriaId { get; set; }
        public string Descripcion { get; set; } = string.Empty; 

    }
}
