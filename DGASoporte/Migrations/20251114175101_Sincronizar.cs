using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DGASoporte.Migrations
{
    /// <inheritdoc />
    public partial class Sincronizar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           /* migrationBuilder.DropForeignKey(name: "FK_Tareas_Estado", table: "Tarea");
            migrationBuilder.DropForeignKey(name: "FK_Tareas_Prioridad", table: "Tarea"); 
            migrationBuilder.DropTable(name: "Estado");
            migrationBuilder.DropTable(name: "Prioridad"); 
            migrationBuilder.DropIndex(name: "IX_Tarea_EstadoId", table: "Tarea"); 
            migrationBuilder.DropIndex(name: "IX_Tarea_PrioridadId", table: "Tarea"); 
            migrationBuilder.DropColumn(name: "EstadoId", table: "Tarea"); 
            migrationBuilder.DropColumn(name: "PrioridadId", table: "Tarea");*/
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EstadoId",
                table: "Tarea",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PrioridadId",
                table: "Tarea",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Estado",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estado", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Prioridad",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prioridad", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tarea_EstadoId",
                table: "Tarea",
                column: "EstadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Tarea_PrioridadId",
                table: "Tarea",
                column: "PrioridadId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tareas_Estado",
                table: "Tarea",
                column: "EstadoId",
                principalTable: "Estado",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tareas_Prioridad",
                table: "Tarea",
                column: "PrioridadId",
                principalTable: "Prioridad",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
