using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DGASoporte.Migrations
{
    /// <inheritdoc />
    public partial class addDvisionS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DivisionId",
                table: "Solicitudes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_DivisionId",
                table: "Solicitudes",
                column: "DivisionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Solicitudes_Divisiones_DivisionId",
                table: "Solicitudes",
                column: "DivisionId",
                principalTable: "Divisiones",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Solicitudes_Divisiones_DivisionId",
                table: "Solicitudes");

            migrationBuilder.DropIndex(
                name: "IX_Solicitudes_DivisionId",
                table: "Solicitudes");

            migrationBuilder.DropColumn(
                name: "DivisionId",
                table: "Solicitudes");
        }
    }
}
