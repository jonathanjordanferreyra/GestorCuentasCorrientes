using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GestorCuentasCorrientes.web.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedTipoPagoyMovimiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MediosPago",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Efectivo" },
                    { 2, "Transferencia" },
                    { 3, "Cheque" },
                    { 4, "E-cheque" },
                    { 5, "Tarjeta débito" },
                    { 6, "Tarjeta crédito" }
                });

            migrationBuilder.InsertData(
                table: "TiposMovimiento",
                columns: new[] { "Id", "Codigo", "Nombre", "Signo" },
                values: new object[,]
                {
                    { 1, "FACT", "Factura", (short)1 },
                    { 2, "REC", "Recibo", (short)-1 },
                    { 3, "NC", "Nota de crédito", (short)-1 },
                    { 4, "ND", "Nota de débito", (short)1 },
                    { 5, "AJU_D", "Ajuste débito", (short)1 },
                    { 6, "AJU_C", "Ajuste crédito", (short)-1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MediosPago",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "MediosPago",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "MediosPago",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "MediosPago",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "MediosPago",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "MediosPago",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "TiposMovimiento",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "TiposMovimiento",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "TiposMovimiento",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "TiposMovimiento",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "TiposMovimiento",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "TiposMovimiento",
                keyColumn: "Id",
                keyValue: 6);
        }
    }
}
