using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DGASoporte.Migrations
{
    /// <inheritdoc />
    public partial class AddDescripcionToTipoIncidencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "TipoIncidencias",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.Sql(@"
            UPDATE TipoIncidencias SET Descripcion = 'Equipo de cómputo no enciende, presenta fallos de hardware o daños físicos.'
            WHERE Nombre = 'ProblemaEquipo';

            UPDATE TipoIncidencias SET Descripcion = 'Accesorios como teclado, mouse, monitor o impresora presentan fallas o no funcionan correctamente.'
            WHERE Nombre = 'ProblemaAccesorios';

            UPDATE TipoIncidencias SET Descripcion = 'Dificultades con la conexión a internet: lentitud, desconexiones o falta total de acceso.'
            WHERE Nombre = 'ProblemaInternet';

            UPDATE TipoIncidencias SET Descripcion = 'Errores o fallos en la instalación, actualización o funcionamiento de un programa o sistema.'
            WHERE Nombre = 'ProblemaSoftware';

            UPDATE TipoIncidencias SET Descripcion = 'Imposibilidad de ingresar a una aplicación, sistema o recurso institucional.'
            WHERE Nombre = 'ProblemaAcceso';

            UPDATE TipoIncidencias SET Descripcion = 'Olvido, bloqueo o error relacionado con contraseñas de usuario en sistemas o correos.'
            WHERE Nombre = 'ProblemaContraseña';

            UPDATE TipoIncidencias SET Descripcion = 'Alertas, virus o vulnerabilidades detectadas que comprometen la seguridad del equipo o la red.'
            WHERE Nombre = 'ProblemaSeguridad';

            UPDATE TipoIncidencias SET Descripcion = 'Fallas en la comunicación interna: correo electrónico, mensajería o servicios de chat corporativo.'
            WHERE Nombre = 'ProblemaComunicacion';

            UPDATE TipoIncidencias SET Descripcion = 'Archivos eliminados, dañados o inaccesibles desde el sistema o la red.'
            WHERE Nombre = 'ProblemaArchivos';

            UPDATE TipoIncidencias SET Descripcion = 'Rendimiento lento del equipo, sobrecarga de procesos o falta de recursos del sistema.'
            WHERE Nombre = 'ProblemaRendimiento';

            UPDATE TipoIncidencias SET Descripcion = 'Solicitud de préstamo, asignación o reemplazo de equipo de cómputo o periféricos.'
            WHERE Nombre = 'SolicitudEquipos';

            UPDATE TipoIncidencias SET Descripcion = 'Otro tipo de problema no contemplado en las categorías anteriores.'
            WHERE Nombre = 'OtroProblema';
        ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "TipoIncidencias");
        }
    }
}
