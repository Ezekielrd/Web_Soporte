using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DGASoporte.Migrations
{
    /// <inheritdoc />
    public partial class tiempoComentario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "InicioContador",
                table: "Tarea",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TiempoInvertido",
                table: "Tarea",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InicioContador",
                table: "Tarea");

            migrationBuilder.DropColumn(
                name: "TiempoInvertido",
                table: "Tarea");
        }
    }
}
