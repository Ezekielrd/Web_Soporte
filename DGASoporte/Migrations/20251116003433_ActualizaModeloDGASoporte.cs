using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DGASoporte.Migrations
{
    /// <inheritdoc />
    public partial class ActualizaModeloDGASoporte : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tarea_TipoServicio_TipoServicioId",
                table: "Tarea");

            migrationBuilder.DropForeignKey(
                name: "FK_TipoServicio_Categoria_CategoriaId",
                table: "TipoServicio");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TipoServicio",
                table: "TipoServicio");

            migrationBuilder.RenameTable(
                name: "TipoServicio",
                newName: "TipoServicios");

            migrationBuilder.RenameIndex(
                name: "IX_TipoServicio_CategoriaId",
                table: "TipoServicios",
                newName: "IX_TipoServicios_CategoriaId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TipoServicios",
                table: "TipoServicios",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tarea_TipoServicios_TipoServicioId",
                table: "Tarea",
                column: "TipoServicioId",
                principalTable: "TipoServicios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TipoServicios_Categoria_CategoriaId",
                table: "TipoServicios",
                column: "CategoriaId",
                principalTable: "Categoria",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tarea_TipoServicios_TipoServicioId",
                table: "Tarea");

            migrationBuilder.DropForeignKey(
                name: "FK_TipoServicios_Categoria_CategoriaId",
                table: "TipoServicios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TipoServicios",
                table: "TipoServicios");

            migrationBuilder.RenameTable(
                name: "TipoServicios",
                newName: "TipoServicio");

            migrationBuilder.RenameIndex(
                name: "IX_TipoServicios_CategoriaId",
                table: "TipoServicio",
                newName: "IX_TipoServicio_CategoriaId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TipoServicio",
                table: "TipoServicio",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tarea_TipoServicio_TipoServicioId",
                table: "Tarea",
                column: "TipoServicioId",
                principalTable: "TipoServicio",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TipoServicio_Categoria_CategoriaId",
                table: "TipoServicio",
                column: "CategoriaId",
                principalTable: "Categoria",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
