using System.ComponentModel.DataAnnotations;

namespace DGASoporte.Models.Enumeradores
{
    public enum EstadoS
    {
        [Display(Name = "Pendiente")]
        Nueva = 0,
        Enviada = 1,     
        Aprobada = 2,  
        Rechazada = 3, 
        EnEspera = 4,
        ConvertidaEnTarea = 5
    }
}
