using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestorCuentasCorrientes.web.Data.Migrations
{
    /// <inheritdoc />
    public partial class MejorasProductosYDetallePresupuesto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Codigo",
                table: "Productos",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnidadMedida",
                table: "Productos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Unidad");

            migrationBuilder.AddColumn<int>(
                name: "ProductoId",
                table: "PresupuestoDetalles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_Codigo",
                table: "Productos",
                column: "Codigo",
                unique: true,
                filter: "[Codigo] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PresupuestoDetalles_ProductoId",
                table: "PresupuestoDetalles",
                column: "ProductoId");

            migrationBuilder.AddForeignKey(
                name: "FK_PresupuestoDetalles_Productos_ProductoId",
                table: "PresupuestoDetalles",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PresupuestoDetalles_Productos_ProductoId",
                table: "PresupuestoDetalles");

            migrationBuilder.DropIndex(
                name: "IX_Productos_Codigo",
                table: "Productos");

            migrationBuilder.DropIndex(
                name: "IX_PresupuestoDetalles_ProductoId",
                table: "PresupuestoDetalles");

            migrationBuilder.DropColumn(
                name: "Codigo",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "UnidadMedida",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "ProductoId",
                table: "PresupuestoDetalles");
        }
    }
}
