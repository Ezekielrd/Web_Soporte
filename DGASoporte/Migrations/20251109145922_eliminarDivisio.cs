using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DGASoporte.Migrations
{
    /// <inheritdoc />
    public partial class eliminarDivisio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Unidad_Division_DivisionId",
                table: "Unidad");

            migrationBuilder.DropTable(
                name: "Division");

            migrationBuilder.DropIndex(
                name: "IX_Unidad_DivisionId",
                table: "Unidad");

            migrationBuilder.DropColumn(
                name: "DivisionId",
                table: "Unidad");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DivisionId",
                table: "Unidad",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Division",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Division", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Unidad_DivisionId",
                table: "Unidad",
                column: "DivisionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Unidad_Division_DivisionId",
                table: "Unidad",
                column: "DivisionId",
                principalTable: "Division",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
