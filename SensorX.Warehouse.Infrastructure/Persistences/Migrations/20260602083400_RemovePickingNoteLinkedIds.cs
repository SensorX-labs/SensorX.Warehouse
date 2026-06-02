using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorX.Warehouse.Infrastructure.Persistences.Migrations
{
    /// <inheritdoc />
    public partial class RemovePickingNoteLinkedIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LinkedSupplyRequestId",
                table: "PickingNotes");

            migrationBuilder.DropColumn(
                name: "LinkedTransferOrderId",
                table: "PickingNotes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LinkedSupplyRequestId",
                table: "PickingNotes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LinkedTransferOrderId",
                table: "PickingNotes",
                type: "uuid",
                nullable: true);
        }
    }
}
