namespace DGASoporte.Models.Enumeradores
{
    public enum TipoIncidencia
    {
        /// Equipo de computo no enciende o tiene problemas físicos
        ProblemaEquipo,
        /// Monitor, teclado, mouse o impresora no funcionan
        ProblemaAccesorios,
        /// No hay conexión a internet o es muy lenta
        ProblemaInternet,
        /// Programas o aplicaciones no funcionan correctamente
        ProblemaSoftware,
        /// No se puede iniciar sesión en el equipo o sistemas
        ProblemaAcceso,
        /// Contraseña olvidada o bloqueada
        ProblemaContraseña,
        /// Virus, malware o seguridad comprometida
        ProblemaSeguridad,
        /// Teléfono o sistemas de comunicación no funcionan
        ProblemaComunicacion,
        /// No se pueden acceder a archivos compartidos o unidades de red
        ProblemaArchivos,
        /// Equipo o programas funcionan muy lento
        ProblemaRendimiento,
        /// Solicitud de nuevo equipo, software o periféricos
        SolicitudEquipos,
        /// Otro tipo de problema no listado
        OtroProblema
    }
}
