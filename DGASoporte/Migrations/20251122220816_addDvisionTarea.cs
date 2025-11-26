using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DGASoporte.Migrations
{
    /// <inheritdoc />
    public partial class addDvisionTarea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tarea_TipoServicios_TipoServicioId",
                table: "Tarea");

            migrationBuilder.AlterColumn<int>(
                name: "TipoServicioId",
                table: "Tarea",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "DivisionId",
                table: "Tarea",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tarea_DivisionId",
                table: "Tarea",
                column: "DivisionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tarea_Divisiones_DivisionId",
                table: "Tarea",
                column: "DivisionId",
                principalTable: "Divisiones",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tarea_TipoServicios_TipoServicioId",
                table: "Tarea",
                column: "TipoServicioId",
                principalTable: "TipoServicios",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tarea_Divisiones_DivisionId",
                table: "Tarea");

            migrationBuilder.DropForeignKey(
                name: "FK_Tarea_TipoServicios_TipoServicioId",
                table: "Tarea");

            migrationBuilder.DropIndex(
                name: "IX_Tarea_DivisionId",
                table: "Tarea");

            migrationBuilder.DropColumn(
                name: "DivisionId",
                table: "Tarea");

            migrationBuilder.AlterColumn<int>(
                name: "TipoServicioId",
                table: "Tarea",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tarea_TipoServicios_TipoServicioId",
                table: "Tarea",
                column: "TipoServicioId",
                principalTable: "TipoServicios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
