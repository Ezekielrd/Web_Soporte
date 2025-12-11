using System.ComponentModel.DataAnnotations;

namespace DGASoporte.Models.Enumeradores
{
    public enum EstadoS
    {
        [Display(Name = "Enviada")]
        Enviada = 1,
        [Display(Name = "Aprovada")]
        Aprobada = 2,
        [Display(Name = "Rechazada")]
        Rechazada = 3,
        [Display(Name ="Cerreda")]
        Cerrada=4
    }
}
