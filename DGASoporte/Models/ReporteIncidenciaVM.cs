namespace DGASoporte.Models
{
    public class ReporteIncidenciaVM
    {
        // 📌 Datos generales
        public int Id { get; set; }
        public string NumeroTicket => $"INC-{Id:D5}";

        public string Titulo { get; set; } = string.Empty;
        public string DescripcionUsuario { get; set; } = string.Empty; // lo que escribió el solicitante

        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaCierre { get; set; }

        public string Solicitante { get; set; } = string.Empty;
        public string Unidad { get; set; } = string.Empty;
        public string? Division { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public string? TipoServicio { get; set; }

        public string Estado { get; set; } = string.Empty;
        public string Prioridad { get; set; } = string.Empty;
        public string? TecnicoAsignado { get; set; }
        public TimeSpan? TiempoResolucion { get; set; }

        // 📌 Diagnóstico (problema)
        public string? ProblemaDetectado { get; set; }  // puedes usar Diagnostico o repetir la descripción técnica

        // 📌 Solución - campos que agregamos a Tarea
        public string? CausaRaiz { get; set; }
        public string? PasosEjecutados { get; set; }
        public string? AjustesRealizados { get; set; }
        public string? Evidencias { get; set; }

        // 📌 Resultado final
        public string? ResultadoFinal { get; set; }

        // 📌 Recomendaciones
        public string? Recomendaciones { get; set; }

        // 📌 Validación del usuario
        public string? UsuarioValida { get; set; }
        public DateTime? FechaValidacion { get; set; }
        public string TiempoInvertidoTexto { get; set; } = "00 h 00 m";

    }
}
