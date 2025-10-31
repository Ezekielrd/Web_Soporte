using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DGASoporte.Migrations
{
    /// <inheritdoc />
    public partial class tablaNivel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Nivel",
                table: "Tecnico",
                newName: "NivelId");

            migrationBuilder.CreateTable(
                name: "Nivel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nivel", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tecnico_NivelId",
                table: "Tecnico",
                column: "NivelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tecnico_Nivel_NivelId",
                table: "Tecnico",
                column: "NivelId",
                principalTable: "Nivel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tecnico_Nivel_NivelId",
                table: "Tecnico");

            migrationBuilder.DropTable(
                name: "Nivel");

            migrationBuilder.DropIndex(
                name: "IX_Tecnico_NivelId",
                table: "Tecnico");

            migrationBuilder.RenameColumn(
                name: "NivelId",
                table: "Tecnico",
                newName: "Nivel");
        }
    }
}
