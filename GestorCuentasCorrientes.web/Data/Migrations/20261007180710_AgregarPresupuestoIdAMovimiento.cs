using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestorCuentasCorrientes.web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarPresupuestoIdAMovimiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PresupuestoId",
                table: "Movimientos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_PresupuestoId",
                table: "Movimientos",
                column: "PresupuestoId",
                unique: true,
                filter: "[PresupuestoId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Movimientos_Presupuestos_PresupuestoId",
                table: "Movimientos",
                column: "PresupuestoId",
                principalTable: "Presupuestos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Movimientos_Presupuestos_PresupuestoId",
                table: "Movimientos");

            migrationBuilder.DropIndex(
                name: "IX_Movimientos_PresupuestoId",
                table: "Movimientos");

            migrationBuilder.DropColumn(
                name: "PresupuestoId",
                table: "Movimientos");
        }
    }
}
