using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DGASoporte.Migrations
{
    public partial class MakeUsuarioIdNotNullInTarea : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Quitar la FK actual
            migrationBuilder.DropForeignKey(
                name: "FK_Tarea_Usuario_UsuarioId",
                table: "Tarea");

            // OPCIONAL pero MUY RECOMENDADO:
            // Si tienes tareas con UsuarioId = NULL, debes asignarles un usuario válido.
            // Cambia 1 por el Id de un Usuario que exista en tu tabla Usuario.
            migrationBuilder.Sql(@"
                DECLARE @UsuarioPorDefecto INT = 1;

                UPDATE Tarea
                SET UsuarioId = @UsuarioPorDefecto
                WHERE UsuarioId IS NULL;
            ");

            // 2) Cambiar la columna a NOT NULL (sin defaultValue: 0)
            migrationBuilder.AlterColumn<int>(
                name: "UsuarioId",
                table: "Tarea",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            // 3) Crear la FK NUEVA SIN CASCADE (NO ACTION)
            migrationBuilder.AddForeignKey(
                name: "FK_Tarea_Usuario_UsuarioId",
                table: "Tarea",
                column: "UsuarioId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tarea_Usuario_UsuarioId",
                table: "Tarea");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioId",
                table: "Tarea",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Tarea_Usuario_UsuarioId",
                table: "Tarea",
                column: "UsuarioId",
                principalTable: "Usuario",
                principalColumn: "Id");
        }
    }
}
