using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DGASoporte.Migrations
{
    /// <inheritdoc />
    public partial class addUsuarioSolivitante : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "Tarea",
                type: "int",
                nullable: true);   // ← true y SIN defaultValue

            migrationBuilder.CreateIndex(
                name: "IX_Tarea_UsuarioId",
                table: "Tarea",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tarea_Usuario_UsuarioId",
                table: "Tarea",
                column: "UsuarioId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);   // o NoAction
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tarea_Usuario_UsuarioId",
                table: "Tarea");

            migrationBuilder.DropIndex(
                name: "IX_Tarea_UsuarioId",
                table: "Tarea");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Tarea");
        }
    }

}
