using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DGASoporte.Migrations
{
    /// <inheritdoc />
    public partial class agregarTSaTarea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Añadir la columna como OPCIONAL (NULLABLE)
            migrationBuilder.AddColumn<int>(
                name: "TipoServicioId",
                table: "Tarea",
                type: "int",
                nullable: true); // Cambiado a TRUE

            // 2. CORREGIR/ACTUALIZAR DATOS EXISTENTES con un ID de TipoServicio VÁLIDO (ej: 1)
            // *** ASUME QUE EL ID=1 EXISTE EN LA TABLA TipoServicio ***
            migrationBuilder.Sql("UPDATE Tarea SET TipoServicioId = 1 WHERE TipoServicioId IS NULL");

            // 3. Añadir el Índice (opcionalmente)
            migrationBuilder.CreateIndex(
                name: "IX_Tarea_TipoServicioId",
                table: "Tarea",
                column: "TipoServicioId");

            // 4. Añadir la Clave Foránea (FK)
            migrationBuilder.AddForeignKey(
                name: "FK_Tarea_TipoServicio_TipoServicioId",
                table: "Tarea",
                column: "TipoServicioId",
                principalTable: "TipoServicio",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // 5. HACER LA COLUMNA OBLIGATORIA (NOT NULL)
            migrationBuilder.AlterColumn<int>(
                name: "TipoServicioId",
                table: "Tarea",
                nullable: false, // Establecemos NOT NULL
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tarea_TipoServicio_TipoServicioId",
                table: "Tarea");

            migrationBuilder.DropIndex(
                name: "IX_Tarea_TipoServicioId",
                table: "Tarea");

            migrationBuilder.DropColumn(
                name: "TipoServicioId",
                table: "Tarea");
        }
    }
}
