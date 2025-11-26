using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DGASoporte.Migrations
{
    /// <inheritdoc />
    public partial class addDivisionId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DivisionId",
                table: "Unidad",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Unidad_DivisionId",
                table: "Unidad",
                column: "DivisionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Unidad_Divisiones_DivisionId",
                table: "Unidad",
                column: "DivisionId",
                principalTable: "Divisiones",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Unidad_Divisiones_DivisionId",
                table: "Unidad");

            migrationBuilder.DropIndex(
                name: "IX_Unidad_DivisionId",
                table: "Unidad");

            migrationBuilder.DropColumn(
                name: "DivisionId",
                table: "Unidad");
        }
    }
}
