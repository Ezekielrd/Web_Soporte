using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DGASoporte.Migrations
{
    /// <inheritdoc />
    public partial class relacionSolictudIncidencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Solicitudes");

            migrationBuilder.AddColumn<int>(
                 name: "TipoIncidenciaId",
                 table: "Solicitudes",
                 nullable: false, 
                 defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_TipoIncidenciaId",
                table: "Solicitudes",
                column: "TipoIncidenciaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Solicitudes_TipoIncidencias_TipoIncidenciaId",
                table: "Solicitudes",
                column: "TipoIncidenciaId",
                principalTable: "TipoIncidencias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revertir los cambios en el método Down
            migrationBuilder.DropForeignKey(
                name: "FK_Solicitudes_TipoIncidencias_TipoIncidenciaId",
                table: "Solicitudes");

            migrationBuilder.DropIndex(
                name: "IX_Solicitudes_TipoIncidenciaId",
                table: "Solicitudes");

            migrationBuilder.DropColumn(
                name: "TipoIncidenciaId",
                table: "Solicitudes");

            // Volver a agregar la columna 'Tipo' original, asumiendo que era un string (VARCHAR)
            // **IMPORTANTE:** Ajusta el tipo de datos si el original no era string
            migrationBuilder.AddColumn<int>(
                name: "Tipo",
                table: "Solicitudes",
                type: "int",
                nullable: false);
        }
    }
}
