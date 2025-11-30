using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DGASoporte.Migrations
{
    /// <inheritdoc />
    public partial class AddCamposSolucionTarea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tarea_Usuario_UsuarioId",
                table: "Tarea");

            migrationBuilder.DropForeignKey(
                name: "FK_Tareas_Categoria",
                table: "Tarea");

            migrationBuilder.AddColumn<string>(
                name: "AjustesRealizados",
                table: "Tarea",
                type: "nvarchar(1500)",
                maxLength: 1500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CausaRaiz",
                table: "Tarea",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCierre",
                table: "Tarea",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaValidacion",
                table: "Tarea",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasosEjecutados",
                table: "Tarea",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Recomendaciones",
                table: "Tarea",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResultadoFinal",
                table: "Tarea",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioValidaId",
                table: "Tarea",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tarea_UsuarioValidaId",
                table: "Tarea",
                column: "UsuarioValidaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tarea_Categoria",
                table: "Tarea",
                column: "CategoriaId",
                principalTable: "Categoria",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tarea_UsuarioSolicitante",
                table: "Tarea",
                column: "UsuarioId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tarea_UsuarioValida",
                table: "Tarea",
                column: "UsuarioValidaId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tarea_Categoria",
                table: "Tarea");

            migrationBuilder.DropForeignKey(
                name: "FK_Tarea_UsuarioSolicitante",
                table: "Tarea");

            migrationBuilder.DropForeignKey(
                name: "FK_Tarea_UsuarioValida",
                table: "Tarea");

            migrationBuilder.DropIndex(
                name: "IX_Tarea_UsuarioValidaId",
                table: "Tarea");

            migrationBuilder.DropColumn(
                name: "AjustesRealizados",
                table: "Tarea");

            migrationBuilder.DropColumn(
                name: "CausaRaiz",
                table: "Tarea");

            migrationBuilder.DropColumn(
                name: "FechaCierre",
                table: "Tarea");

            migrationBuilder.DropColumn(
                name: "FechaValidacion",
                table: "Tarea");

            migrationBuilder.DropColumn(
                name: "PasosEjecutados",
                table: "Tarea");

            migrationBuilder.DropColumn(
                name: "Recomendaciones",
                table: "Tarea");

            migrationBuilder.DropColumn(
                name: "ResultadoFinal",
                table: "Tarea");

            migrationBuilder.DropColumn(
                name: "UsuarioValidaId",
                table: "Tarea");

            migrationBuilder.AddForeignKey(
                name: "FK_Tarea_Usuario_UsuarioId",
                table: "Tarea",
                column: "UsuarioId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tareas_Categoria",
                table: "Tarea",
                column: "CategoriaId",
                principalTable: "Categoria",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
