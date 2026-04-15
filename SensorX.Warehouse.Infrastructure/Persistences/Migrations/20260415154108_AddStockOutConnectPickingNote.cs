using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorX.Warehouse.Infrastructure.Persistences.Migrations
{
    /// <inheritdoc />
    public partial class AddStockOutConnectPickingNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_StockOuts_PickingNoteId",
                table: "StockOuts",
                column: "PickingNoteId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StockOuts_PickingNotes_PickingNoteId",
                table: "StockOuts",
                column: "PickingNoteId",
                principalTable: "PickingNotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockOuts_PickingNotes_PickingNoteId",
                table: "StockOuts");

            migrationBuilder.DropIndex(
                name: "IX_StockOuts_PickingNoteId",
                table: "StockOuts");
        }
    }
}
