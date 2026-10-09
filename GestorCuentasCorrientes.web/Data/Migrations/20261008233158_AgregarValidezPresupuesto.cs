using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestorCuentasCorrientes.web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarValidezPresupuesto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaVencimiento",
                table: "Presupuestos",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaVencimiento",
                table: "Presupuestos");
        }
    }
}
