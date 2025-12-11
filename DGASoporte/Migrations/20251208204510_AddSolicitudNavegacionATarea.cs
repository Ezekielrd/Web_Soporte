using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DGASoporte.Migrations
{
    /// <inheritdoc />
    public partial class AddSolicitudNavegacionATarea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Tarea_SolicitudId",
                table: "Tarea",
                column: "SolicitudId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tarea_Solicitudes_SolicitudId",
                table: "Tarea",
                column: "SolicitudId",
                principalTable: "Solicitudes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tarea_Solicitudes_SolicitudId",
                table: "Tarea");

            migrationBuilder.DropIndex(
                name: "IX_Tarea_SolicitudId",
                table: "Tarea");
        }
    }
}
