using System.ComponentModel.DataAnnotations;

namespace DGASoporte.Models.Enumeradores
{
    public enum EstadoT
    {
        [Display(Name = "Nueva")]
        Nuevo = 1,
        [Display(Name = "Asiganda")]
        Asignado = 2,
        [Display(Name = "En Proceso")]
        EnProceso = 3,
        [Display(Name = "En Espera")]
        EnEspera = 4,
        [Display(Name = "Resuelta")]
        Resuelta = 5,
        [Display(Name = "ReAbierta")]
        Reabierto = 6,
        [Display(Name = "Escalada")]
        Escalado = 7,
        [Display(Name = "En Pausa")]
        Pausado = 8
    }
}
